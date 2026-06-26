namespace Api.Security;

public class HmacAuthOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ToleranceMinutes { get; set; } = 5;
}
