namespace Infrastructure.Auth;

public class JwtSettings
{
    public const string SectionName = "JWT";
    public string Issuer { get; init; } = "erp-local";
    public string Audience { get; init; } = "erp-web";
    public string Key { get; init; } = string.Empty; // must be >= 32 chars in prod
    public int ExpiryMinutes { get; init; } = 120;
}

