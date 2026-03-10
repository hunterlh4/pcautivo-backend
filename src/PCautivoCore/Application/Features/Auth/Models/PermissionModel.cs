namespace PCautivoCore.Application.Features.Auth.Models;

public class PermissionModel
{
    public string? Name { get; set; }
    public string? Controller { get; set; }
    public string? ActionName { get; set; }
    public string? HttpMethod { get; set; }
    public string? ActionType { get; set; }
}