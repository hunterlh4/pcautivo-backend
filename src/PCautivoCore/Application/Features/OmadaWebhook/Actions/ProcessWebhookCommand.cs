using MediatR;
using Microsoft.Extensions.Logging;
using PCautivoCore.Application.Features.OmadaWebhook.Dtos;
using PCautivoCore.Domain.Enums;
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

        var eventTime = payload.Time.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(payload.Time.Value).UtcDateTime
            : DateTime.UtcNow;
        eventTime = NormalizeForPersistence(eventTime);

        if (string.IsNullOrWhiteSpace(payload.ClientMac))
        {
            return Errors.BadRequest("MAC del cliente requerida.");
        }

        var sessionType = payload.EventType switch
        {
            { } t when t.Contains("CONNECTED") || t.Contains("ONLINE") => DeviceSessionType.Entrada,
            { } t when t.Contains("DISCONNECTED") || t.Contains("OFFLINE") => DeviceSessionType.Salida,
            _ => (DeviceSessionType?)null
        };

        if (sessionType is null)
        {
            return Errors.BadRequest($"Evento no soportado: {payload.EventType}");
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
                CreatedAt = eventTime
            };
            deviceId = await _deviceRepository.AddDeviceAsync(newDevice);
        }

        var session = new DeviceSession
        {
            DeviceId = deviceId.Value,
            SessionType = sessionType.Value,
            EventTime = eventTime
        };

        _logger.LogInformation(
            "Webhook Omada: guardando sesión. DeviceId: {DeviceId}, Tipo: {SessionType}, EventTime: {EventTime}.",
            session.DeviceId,
            session.SessionType,
            session.EventTime);

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
