using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public UserType UserType { get; set; } 
    public bool SuperUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public UserDetail? Detail { get; set; }
}