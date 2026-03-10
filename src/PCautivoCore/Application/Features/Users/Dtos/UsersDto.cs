using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Application.Features.Users.Dtos;

public record UsersDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public UserType UserType { get; set; }
    public int PropertiesCount { get; set; }
    public UsersDetailDto? Detail { get; set; }
    public IEnumerable<UsersRoleDto> Roles { get; set; } = [];
}

public record UsersDetailDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CountryCode { get; set; }
}

public record UsersRoleDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}