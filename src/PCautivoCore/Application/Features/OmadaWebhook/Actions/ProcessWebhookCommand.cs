using MediatR;
using Microsoft.Extensions.Logging;
using PCautivoCore.Application.Features.OmadaWebhook.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Shared.Responses;

namespace PCautivoCore.Application.Features.OmadaWebhook.Actions;

public record ProcessWebhookCommand(OmadaWebhookPayloadDto Payload) : IRequest<Result>;

public class ProcessWebhookCommandHandler : IRequestHandler<ProcessWebhookCommand, Result>
{
    private readonly ILogger<ProcessWebhookCommandHandler> _logger;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceSessionRepository _deviceSessionRepository;

    public ProcessWebhookCommandHandler(
        ILogger<ProcessWebhookCommandHandler> logger,
        IDeviceRepository deviceRepository,
        IDeviceSessionRepository deviceSessionRepository)
    {
        _logger = logger;
        _deviceRepository = deviceRepository;
        _deviceSessionRepository = deviceSessionRepository;
    }

    public async Task<Result> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;

        if (string.IsNullOrWhiteSpace(payload.ClientMac))
        {
            return Errors.BadRequest("MAC del cliente requerida.");
        }

        var normalizedMac = NormalizeMacAddress(payload.ClientMac);

        var deviceId = await _deviceRepository.GetDeviceByMacAsync(normalizedMac);

        if (deviceId is null || deviceId == 0)
        {
            _logger.LogInformation("Webhook Omada: guardando nuevo dispositivo para MAC {ClientMac}.", normalizedMac);

            var newDevice = new Device
            {
                MacAddress = normalizedMac,
                Dni = null,
                CreatedAt = DateTime.UtcNow
            };
            deviceId = await _deviceRepository.AddDeviceAsync(newDevice);
        }

        var session = new DeviceSession
        {
            DeviceId = deviceId.Value,
            OmadaId = null,
            StartTime = payload.Time.HasValue
                ? NormalizeForPersistence(DateTimeOffset.FromUnixTimeMilliseconds(payload.Time.Value).UtcDateTime)
                : DateTime.UtcNow,
            EndTime = null,
            DurationSeconds = null
        };

        _logger.LogInformation(
            "Webhook Omada: guardando sesión. DeviceId: {DeviceId}, StartTime: {StartTime}.",
            session.DeviceId,
            session.StartTime);

        await _deviceSessionRepository.RegisterSessionAsync(session);

        _logger.LogInformation("Webhook Omada: sesión guardada correctamente para MAC {ClientMac}.", normalizedMac);

        return Result.Success();
    }

    private static DateTime NormalizeForPersistence(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static string NormalizeMacAddress(string macAddress)
    {
        return macAddress
            .Trim()
            .Replace(':', '-')
            .Replace('.', '-')
            .ToUpperInvariant();
    }
}
