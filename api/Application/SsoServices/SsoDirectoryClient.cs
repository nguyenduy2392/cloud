using System.Text.Json;
using Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.SsoServices
{
    public class SsoResolvedOrgMember
    {
        public Guid SsoUserId { get; set; }
        public Guid OrgId { get; set; }
        public string OrgTitle { get; set; } = string.Empty;
        public string? OrgShortName { get; set; }
        public Guid? OrgRoleId { get; set; }
        public string? OrgRoleName { get; set; }
    }

    public interface ISsoDirectoryClient
    {
        /// <summary>Toàn bộ membership (org/chức danh) đang active của 1 tenant bên SSO. Best-effort — trả rỗng nếu lỗi/không kết nối được.</summary>
        Task<List<SsoResolvedOrgMember>> GetResolvedMembersAsync(string tenantName);
    }

    public class SsoDirectoryClient : ISsoDirectoryClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SsoSettings _settings;
        private readonly ILogger<SsoDirectoryClient> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public SsoDirectoryClient(IHttpClientFactory httpClientFactory, IOptions<SsoSettings> settings, ILogger<SsoDirectoryClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<List<SsoResolvedOrgMember>> GetResolvedMembersAsync(string tenantName)
        {
            if (string.IsNullOrWhiteSpace(_settings.BaseUrl) || string.IsNullOrWhiteSpace(tenantName))
                return new List<SsoResolvedOrgMember>();

            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{_settings.BaseUrl.TrimEnd('/')}/orgs/members/resolved?tenantName={Uri.EscapeDataString(tenantName)}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("SSO GetResolvedMembers failed ({Status}): {Body}", response.StatusCode, body);
                    return new List<SsoResolvedOrgMember>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SsoResolvedOrgMember>>(json, _jsonOpts) ?? new List<SsoResolvedOrgMember>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSO GetResolvedMembers error for tenant {Tenant}", tenantName);
                return new List<SsoResolvedOrgMember>();
            }
        }
    }
}
