namespace PCautivoCore.Domain.Models;

public class Brand
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}