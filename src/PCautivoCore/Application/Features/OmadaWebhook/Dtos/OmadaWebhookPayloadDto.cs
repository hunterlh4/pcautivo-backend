namespace PCautivoCore.Application.Features.OmadaWebhook.Dtos;

public class OmadaWebhookPayloadDto
{
    public string EventType { get; set; } = string.Empty;
    public string ClientMac { get; set; } = string.Empty;
    public string? ClientIp { get; set; }
    public string? ApMac { get; set; }
    public string? SiteId { get; set; }
    public long? Time { get; set; }
}
