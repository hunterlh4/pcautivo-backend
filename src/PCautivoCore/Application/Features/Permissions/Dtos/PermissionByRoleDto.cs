namespace PCautivoCore.Application.Features.Permissions.Dtos;

public class PermissionByRoleDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Controller { get; set; }
    public string? ActionName { get; set; }
    public string? HttpMethod { get; set; }
    public string? ActionType { get; set; }
    public bool Status { get; set; }
}