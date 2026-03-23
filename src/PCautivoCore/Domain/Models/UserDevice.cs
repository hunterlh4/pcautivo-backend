namespace PCautivoCore.Domain.Models;

public class UserDevice
{
    public int UserId { get; set; }
    public int DeviceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
