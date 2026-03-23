namespace PCautivoCore.Domain.Models;

public class DeviceSession
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string? OmadaId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationSeconds { get; set; }
}
