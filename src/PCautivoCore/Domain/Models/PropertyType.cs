namespace PCautivoCore.Domain.Models;

public class PropertyType
{
    public int Id { get; set; }
    public int? ExternalId { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
