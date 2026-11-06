namespace TrabajaDecente.Api.Services;

public class JwtOptions
{
    public string Issuer { get; set; } = "TrabajaDecente";
    public string Audience { get; set; } = "TrabajaDecente.Web";
    public string Key { get; set; } = "";
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 7;
}
