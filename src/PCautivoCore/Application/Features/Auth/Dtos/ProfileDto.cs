namespace PCautivoCore.Application.Features.Auth.Dtos;

public class ProfileDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CountryCode { get; set; }
}
