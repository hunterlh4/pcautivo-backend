using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public class DeviceSession
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public DeviceSessionType SessionType { get; set; } = DeviceSessionType.Entrada; // 1 = ENTRADA, 2 = SALIDA
    public string? OmadaId { get; set; }
    public DateTime EventTime { get; set; }
}
