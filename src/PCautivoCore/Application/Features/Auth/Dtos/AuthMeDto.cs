namespace PCautivoCore.Application.Features.Auth.Dtos;

public record AuthMeDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public IEnumerable<string> Permissions { get; set; } = [];
}