namespace PCautivoCore.Infrastructure.Models.Omada;

public class OmadaWebhookPayload
{
    public string EventType { get; set; } = string.Empty;
    public string ClientMac { get; set; } = string.Empty;
    public string? ClientIp { get; set; }
    public string? ApMac { get; set; }
    public string? SiteId { get; set; }
    public long? Time { get; set; }
}
