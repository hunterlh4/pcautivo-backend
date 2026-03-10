namespace PCautivoCore.Domain.Models;

public class Permission
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Controller { get; set; }
    public string? ActionName { get; set; }
    public string? HttpMethod { get; set; }
    public string? ActionType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}