namespace PCautivoCore.Domain.Models;

public class Device
{
    public int Id { get; set; }
    public string MacAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
