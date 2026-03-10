namespace PCautivoCore.Domain.Models;

public class UserProperty
{
    public int UserId { get; set; }
    public int PropertyId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
