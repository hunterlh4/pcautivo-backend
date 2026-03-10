using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PCautivoCore.Application.Features.CaptiveAuth.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using PCautivoCore.Shared.Utils;

namespace PCautivoCore.Application.Features.CaptiveAuth.Actions;

/// <summary>
/// Comando de autenticación para el portal cautivo (tipo: Servidor de portal externo).
/// Valida credenciales contra la BD, luego notifica a Omada vía extPortal/auth usando el token
/// 't' que el controlador inyectó en el redirect para liberar la sesión del cliente.
/// </summary>
public record CaptiveLoginCommand : IRequest<Result<CaptiveLoginDto>>
{
    /// <summary>Nombre de usuario.</summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>Contraseña del usuario.</summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>MAC del dispositivo cliente (ej: aa:bb:cc:dd:ee:ff).</summary>
    public string ClientMac { get; init; } = string.Empty;

    /// <summary>MAC del access point.</summary>
    public string ApMac { get; init; } = string.Empty;

    /// <summary>Nombre del SSID.</summary>
    public string SsidName { get; init; } = string.Empty;

    /// <summary>ID de radio del AP: 0 = 2.4 GHz, 1 = 5 GHz.</summary>
    public int RadioId { get; init; } = 0;

    /// <summary>Token de sesión (?t=...) que Omada inyecta en el redirect al portal externo.</summary>
    public long? T { get; init; }

    /// <summary>IP del cliente (?clientIp=... del redirect URL de Omada).</summary>
    public string? ClientIp { get; init; }

    /// <summary>ID del sitio Omada (?site=... del redirect, ej: 638eff71cbfdfc3b05c3ef36).</summary>
    public string? Site { get; init; }

    /// <summary>URL original a la que intentaba acceder el cliente antes del redirect.</summary>
    public string? OriginUrl { get; init; }

    // ──────────────────────────────────────────────────────────
    // Validador
    // ──────────────────────────────────────────────────────────

    public sealed class Validator : AbstractValidator<CaptiveLoginCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El nombre de usuario es requerido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.");

            RuleFor(x => x.ClientMac)
                .NotEmpty().WithMessage("La MAC del cliente es requerida.")
                .Matches(@"^([0-9A-Fa-f]{2}[:\-.]){5}([0-9A-Fa-f]{2})$")
                .WithMessage("Formato de MAC inválido (ej: aa:bb:cc:dd:ee:ff).");

            RuleFor(x => x.ApMac)
                .NotEmpty().WithMessage("La MAC del AP es requerida.");

            RuleFor(x => x.SsidName)
                .NotEmpty().WithMessage("El nombre del SSID es requerido.");
        }
    }

 
    internal sealed class Handler(
        IUserRepository userRepository,
        IOmadaService omadaService,
        IJwtUtil jwtUtil,
        ILogger<Handler> logger) : IRequestHandler<CaptiveLoginCommand, Result<CaptiveLoginDto>>
    {
        public async Task<Result<CaptiveLoginDto>> Handle(
            CaptiveLoginCommand request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "─── CaptiveLogin REQUEST ─── User={User} ClientMac={ClientMac} ApMac={ApMac} " +
                "SsidName={SsidName} RadioId={RadioId} T={T} ClientIp={ClientIp} Site={Site} OriginUrl={OriginUrl}",
                request.Username, request.ClientMac, request.ApMac,
                request.SsidName, request.RadioId, request.T, request.ClientIp, request.Site, request.OriginUrl);
            // 1. Validar que el usuario existe
            var user = await userRepository.GetUserByUsername(request.Username);

            if (user is null)
                return Errors.Unauthorized("Usuario o contraseña incorrectos 1.");

            // 2. Verificar contraseña
            bool passwordValid = PasswordUtil.VerifyPassword(request.Password, user.PasswordHash);

            if (!passwordValid)
                return Errors.Unauthorized("Usuario o contraseña incorrectos 2.");

            // 3. Autorizar MAC en Omada vía extPortal/auth (Servidor de portal externo)
            //    Omada identifica la sesión pendiente por el token 't' del redirect.
            bool omadaOk = await omadaService.AuthorizeClientAsync(
                target:      string.Empty,
                targetPort:  0,
                clientMac:   request.ClientMac,
                apMac:       request.ApMac,
                ssidName:    request.SsidName,
                radioId:     request.RadioId,
                site:        request.Site,
                t:           request.T,
                clientIp:    request.ClientIp,
                redirectUrl: request.OriginUrl);

            if (!omadaOk)
                return Errors.BadRequest("No se pudo autorizar el dispositivo en el controlador WiFi. Intente nuevamente.");

            // 4. Generar token JWT
            string accessToken = jwtUtil.GenerateToken(user.Id.ToString());
            int expiresIn = jwtUtil.GetExpiresIn();

            var response = new CaptiveLoginDto
            {
                AccessToken = accessToken,
                TokenType   = "Bearer",
                ExpiresIn   = expiresIn,
                Username    = user.Username,
                LandingUrl  = request.OriginUrl
            };

            logger.LogInformation(
                "─── CaptiveLogin RESPONSE ─── Username={Username} ExpiresIn={ExpiresIn} LandingUrl={LandingUrl}",
                response.Username, response.ExpiresIn, response.LandingUrl);

            return response;
        }
    }
}
