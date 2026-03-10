namespace PCautivoCore.Application.Features.UserProperties.Dtos;

public record UserPropertyDto
{
    public int PropertyId { get; init; }
    public int? ExternalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Country { get; init; }
    public string? City { get; init; }
    public DateTimeOffset AssignedAt { get; init; }
}
