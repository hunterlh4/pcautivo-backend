namespace PCautivoCore.Domain.Models;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}