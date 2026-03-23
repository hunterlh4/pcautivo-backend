namespace PCautivoCore.Infrastructure.Models.Omada;

/// <summary>
/// Sesion de cliente obtenida desde la API de hotspot de Omada.
/// </summary>
public class OmadaHotspotClientSession
{
    public string? Id { get; set; }
    public string ClientMac { get; set; } = string.Empty;
    public string? ClientIp { get; set; }
    public string? ClientName { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationSeconds { get; set; }
    public string RawJson { get; set; } = string.Empty;
}
