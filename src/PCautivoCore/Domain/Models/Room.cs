namespace PCautivoCore.Domain.Models;

public class Room
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int PropertyId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Property? Property { get; set; }
}
