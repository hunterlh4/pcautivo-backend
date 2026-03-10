using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Application.Features.Users.Models;

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public UserType? UserType { get; set; }
}