namespace PCautivoApi.Shared.Settings;

public class HostSetting
{
    public required string WebHostUrl { get; set; }
    public string? AllowedHosts { get; set; }
    public required string PolicyName { get; set; }
}
