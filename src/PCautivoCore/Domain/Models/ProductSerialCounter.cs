namespace PCautivoCore.Domain.Models;

public class ProductSerialCounter
{
    public int ProductId { get; set; }
    public int LastSerialNumber { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
