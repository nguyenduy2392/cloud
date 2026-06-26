using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Security;

public class HmacAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HmacAuthOptions _options;
    private readonly ILogger<HmacAuthMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    // Scenario A: system endpoints (no JWT needed)
    private static readonly string[] SystemPrefixes =
    {
        "/api/system/database/initialize",
        "/api/system/migration/run",
        "/api/system/backfill-keywords"
    };

    // Scenario B: cloud API endpoints proxied from HRM BE (on-behalf-of)
    private static readonly string[] ProxyPrefixes =
    {
        "/api/folders",
        "/api/files",
        "/api/resource-permissions",
        "/api/user-storage"
    };

    public HmacAuthMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<HmacAuthMiddleware> logger, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _options = configuration.GetSection("HmacAuth").Get<HmacAuthOptions>() ?? new HmacAuthOptions();
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        // Determine which scenario applies
        var isSystemPath = SystemPrefixes.Any(p =>
            request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

        var isProxyPath = ProxyPrefixes.Any(p =>
            request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

        // Only intercept if HMAC headers are present on proxy paths (allows normal JWT requests through)
        var hasHmacHeaders = request.Headers.ContainsKey("X-Api-Key")
                          && request.Headers.ContainsKey("X-Timestamp")
                          && request.Headers.ContainsKey("X-Signature");

        if (!isSystemPath && !(isProxyPath && hasHmacHeaders))
        {
            await _next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            _logger.LogWarning("HMAC auth options not configured properly. ApiKey or SecretKey is empty. Skipping HMAC validation.");
            await _next(context);
            return;
        }

        if (!request.Headers.TryGetValue("X-Api-Key", out var apiKey) ||
            !request.Headers.TryGetValue("X-Timestamp", out var timestampHeader) ||
            !request.Headers.TryGetValue("X-Signature", out var signatureHeader))
        {
            _logger.LogWarning("HMAC auth failed: missing headers. X-Api-Key: {HasApiKey}, X-Timestamp: {HasTimestamp}, X-Signature: {HasSignature}",
                request.Headers.ContainsKey("X-Api-Key"),
                request.Headers.ContainsKey("X-Timestamp"),
                request.Headers.ContainsKey("X-Signature"));
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing HMAC headers.");
            return;
        }

        if (!string.Equals(apiKey, _options.ApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("HMAC auth failed: invalid API key. Provided: {ProvidedApiKey}", apiKey.ToString());
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid API key.");
            return;
        }

        if (!DateTimeOffset.TryParse(timestampHeader, out var timestamp))
        {
            _logger.LogWarning("HMAC auth failed: invalid timestamp. Raw value: {Timestamp}", timestampHeader.ToString());
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid timestamp.");
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (Math.Abs((now - timestamp).TotalMinutes) > _options.ToleranceMinutes)
        {
            _logger.LogWarning("HMAC auth failed: timestamp too far from current time. Now(UTC): {Now}, Timestamp: {Timestamp}, ToleranceMinutes: {ToleranceMinutes}",
                now, timestamp, _options.ToleranceMinutes);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Timestamp too far from current time.");
            return;
        }

        // Read body for signature (skip multipart/form-data — binary body can't be part of signature)
        string body = string.Empty;
        var contentType = request.ContentType ?? string.Empty;
        if (request.ContentLength > 0 && !contentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        var path = request.Path.Value ?? string.Empty;
        var query = request.QueryString.HasValue
            ? request.QueryString.Value!.TrimStart('?')
            : string.Empty;

        var computedSignature = HmacSignatureHelper.ComputeSignature(
            _options.SecretKey,
            request.Method,
            path,
            query,
            timestampHeader!,
            body);

        if (!AreEqualSlow(signatureHeader!, computedSignature))
        {
            _logger.LogWarning("HMAC auth failed: invalid signature. Provided: {ProvidedSignature}, Computed: {ComputedSignature}, Path: {Path}, Query: {Query}",
                Truncate(signatureHeader!, 100),
                Truncate(computedSignature, 100),
                path,
                query);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid HMAC signature.");
            return;
        }

        // Scenario B: proxy request with on-behalf-of user
        if (isProxyPath)
        {
            if (!request.Headers.TryGetValue("X-On-Behalf-Of", out var onBehalfOf) ||
                string.IsNullOrWhiteSpace(onBehalfOf))
            {
                _logger.LogWarning("HMAC proxy auth failed: missing X-On-Behalf-Of header for path {Path}.", path);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing X-On-Behalf-Of header.");
                return;
            }

            if (!request.Headers.TryGetValue("X-Database", out var database) ||
                string.IsNullOrWhiteSpace(database))
            {
                _logger.LogWarning("HMAC proxy auth failed: missing X-Database header for path {Path}.", path);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing X-Database header.");
                return;
            }

            // X-On-Behalf-Of chứa SsoId (chung giữa các app). Cần resolve → Cloud AppUser.Id.
            if (!Guid.TryParse(onBehalfOf, out var ssoId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid X-On-Behalf-Of (not a valid GUID).");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Core.AppDbContext>();
            var connAccessor = scope.ServiceProvider.GetRequiredService<Core.IAppContextAccessor>();
            db.Database.SetConnectionString(connAccessor.GetConnectionString(database!));

            var cloudUserId = await db.Users
                .Where(u => u.SsoId == ssoId && !u.IsDeleted)
                .Select(u => (Guid?)u.Id)
                .FirstOrDefaultAsync();

            if (cloudUserId == null)
            {
                _logger.LogWarning("HMAC proxy: no Cloud AppUser found for SsoId {SsoId} in database {Database}.", ssoId, database.ToString());
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("User not found in Cloud for this SsoId.");
                return;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, cloudUserId.Value.ToString()),
                new Claim("Database", database!),
            };
            var identity = new ClaimsIdentity(claims, "HMAC");
            context.User = new ClaimsPrincipal(identity);

            _logger.LogInformation(
                "HMAC proxy auth succeeded. SsoId: {SsoId}, CloudUserId: {CloudUserId}, Database: {Database}, Path: {Path}",
                ssoId, cloudUserId.Value, database.ToString(), path);

        }

        await _next(context);
    }

    private static bool AreEqualSlow(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);

        if (aBytes.Length != bBytes.Length) return false;

        var result = 0;
        for (int i = 0; i < aBytes.Length; i++)
        {
            result |= aBytes[i] ^ bBytes[i];
        }

        return result == 0;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}
