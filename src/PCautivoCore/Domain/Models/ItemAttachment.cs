namespace PCautivoCore.Domain.Models;

public class ItemAttachment
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string? FilePath { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}