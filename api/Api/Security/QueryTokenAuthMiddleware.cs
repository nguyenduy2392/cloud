namespace Api.Security;

public class QueryTokenAuthMiddleware
{
    private readonly RequestDelegate _next;

    public QueryTokenAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Authorization")
            && context.Request.Query.TryGetValue("t", out var token)
            && !string.IsNullOrWhiteSpace(token))
        {
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }

        await _next(context);
    }
}
