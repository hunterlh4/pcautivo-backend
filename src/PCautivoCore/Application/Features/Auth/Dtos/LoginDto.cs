namespace PCautivoCore.Application.Features.Auth.Dtos;

public record LoginDto
{
    public required string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public required string AccessToken { get; set; }
}