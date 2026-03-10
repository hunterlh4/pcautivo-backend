namespace PCautivoCore.Shared.Settings;

public class JwtSetting
{
    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpiresIn { get; set; }
}