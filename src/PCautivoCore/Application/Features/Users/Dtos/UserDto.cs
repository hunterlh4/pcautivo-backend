using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Application.Features.Users.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public UserType UserType { get; set; }
    public IEnumerable<UserRoleDto> Roles { get; set; } = [];
}

public class UserRoleDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}