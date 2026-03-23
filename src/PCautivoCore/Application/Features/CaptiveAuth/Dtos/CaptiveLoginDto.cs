namespace PCautivoCore.Application.Features.CaptiveAuth.Dtos;

/// <summary>
/// Respuesta devuelta al cliente tras una autenticación exitosa en el portal cautivo.
/// </summary>
public record CaptiveLoginDto
{
    /// <summary>Token JWT de acceso.</summary>
    public required string AccessToken { get; init; }

    /// <summary>Tipo de token (Bearer).</summary>
    public required string TokenType { get; init; }

    /// <summary>Segundos hasta expiración del token.</summary>
    public int ExpiresIn { get; init; }

    /// <summary>DNI autenticado.</summary>
    public required string Dni { get; init; }

    /// <summary>URL original a la que será redirigido el cliente tras la autorización.</summary>
    public string? LandingUrl { get; init; }
}
