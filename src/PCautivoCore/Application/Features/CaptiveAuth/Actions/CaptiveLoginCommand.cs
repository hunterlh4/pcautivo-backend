using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCautivoCore.Application.Features.CaptiveAuth.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Infrastructure.Settings;
using PCautivoCore.Shared.Responses;
using PCautivoCore.Shared.Utils;
using System.Text;
using System.Text.Json;

namespace PCautivoCore.Application.Features.CaptiveAuth.Actions;

public record CaptiveLoginCommand : IRequest<Result<CaptiveLoginDto>>
{
    /// <summary>
    /// DNI
    /// MAC del cliente (ej: aa:bb:cc:dd:ee:ff).
    /// MAC del access point.
    /// Nombre del SSID.
    /// ID de radio del AP: 0 = 2.4 GHz, 1 = 5 GHz.
    /// Token de sesión (?t=...) que Omada inyecta en el redirect al portal externo.
    /// IP del cliente (?clientIp=... del redirect URL de Omada).
    /// ID del sitio Omada (?site=... del redirect, ej: 638eff71cbfdfc3b05c3ef36).
    /// URL original a la que intentaba acceder el cliente antes del redirect.
    /// </summary>
    public string Dni { get; init; } = string.Empty;
    public string ClientMac { get; init; } = string.Empty;
    public string ApMac { get; init; } = string.Empty;
    public string SsidName { get; init; } = string.Empty;
    public int RadioId { get; init; } = 0;
    public long? T { get; init; }
    public string? ClientIp { get; init; }
    public string? Site { get; init; }
    public string? OriginUrl { get; init; }

    // ──────────────────────────────────────────────────────────
    // Validador
    // ──────────────────────────────────────────────────────────

    public sealed class Validator : AbstractValidator<CaptiveLoginCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Dni)
                .NotEmpty().WithMessage("El DNI es requerido.")
                .Matches(@"^\d{8}$").WithMessage("El DNI debe tener 8 digitos.");

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
        IOptions<IasSettings> iasOptions,
        IHttpClientFactory httpClientFactory,
        IOmadaService omadaService,
        IDeviceRepository deviceRepository,
        IJwtUtil jwtUtil) : IRequestHandler<CaptiveLoginCommand, Result<CaptiveLoginDto>>
    {
        public async Task<Result<CaptiveLoginDto>> Handle(
            CaptiveLoginCommand request,
            CancellationToken cancellationToken)
        {
            var normalizedMac = NormalizeMacAddress(request.ClientMac);
            var normalizedDni = request.Dni.Trim();

            // 1. Validar que el DNI existe en el IAS
            var iasError = await ValidateDniInIasAsync(normalizedDni, cancellationToken);
            if (iasError is not null)
            {
                return iasError;
            }

            // 2. Preguntar Omada identifica la sesión pendiente por el token 't' del redirect.
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
            {
                return Errors.BadRequest("No se pudo autorizar el dispositivo en el controlador WiFi. Intente nuevamente.");
            }

            var deviceId = await deviceRepository.GetDeviceByMacAsync(normalizedMac);
            if (!deviceId.HasValue || deviceId.Value == 0)
            {
                await deviceRepository.AddDeviceAsync(new Device
                {
                    MacAddress = normalizedMac,
                    Dni = normalizedDni,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                await deviceRepository.UpdateDeviceDniByMacAsync(normalizedMac, normalizedDni);
            }

            string accessToken = jwtUtil.GenerateToken(normalizedDni);
            int expiresIn = jwtUtil.GetExpiresIn();

            var response = new CaptiveLoginDto
            {
                AccessToken = accessToken,
                TokenType   = "Bearer",
                ExpiresIn   = expiresIn,
                Dni         = normalizedDni,
                LandingUrl  = request.OriginUrl ?? "http://connectivitycheck.gstatic.com/generate_204"
            };

            return response;
        }

        private static string NormalizeMacAddress(string macAddress)
        {
            return macAddress
                .Trim()
                .Replace(':', '-')
                .Replace('.', '-')
                .ToUpperInvariant();
        }

        private async Task<Error?> ValidateDniInIasAsync(string dni, CancellationToken cancellationToken)
        {
            var iasBaseConnection = iasOptions.Value.Connection;
            if (string.IsNullOrWhiteSpace(iasBaseConnection))
            {
                return Errors.BadRequest("No esta configurada la conexion IAS.");
            }

            var iasApiUrl = $"{iasBaseConnection}AsistenciaCliente/GetListadoClienteCoincidencia";
            var requestBody = new
            {
                coincidencia = dni
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var httpClient = httpClientFactory.CreateClient();
            var httpResponse = await httpClient.PostAsync(iasApiUrl, httpContent, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Errors.BadRequest($"Error al consultar API de IAS: {httpResponse.StatusCode}");
            }

            var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var iasResponse = JsonSerializer.Deserialize<IasApiResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var dniExists = iasResponse?.Data != null && iasResponse.Data.Any();
            if (!dniExists)
            {
                return Errors.Unauthorized("Usuario no encontrado");
            }

            return null;
        }
    }
}
