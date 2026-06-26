using System.Security.Cryptography;
using System.Text;

namespace Api.Security;

public static class HmacSignatureHelper
{
    public static string ComputeSignature(
        string secretKey,
        string httpMethod,
        string path,
        string query,
        string timestamp,
        string body = "")
    {
        var canonical = $"{httpMethod.ToUpperInvariant()}\n{path}\n{query}\n{timestamp}\n{body}";
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(canonical);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return Convert.ToBase64String(hash);
    }
}
