namespace PCautivoCore.Domain.Models;

public class RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}