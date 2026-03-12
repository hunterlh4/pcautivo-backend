using MediatR;
using Microsoft.Extensions.Logging;
using PCautivoCore.Domain.Enums;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Infrastructure.Models.Omada;
using PCautivoCore.Shared.Responses;

namespace PCautivoCore.Application.Features.Omada.Actions;

public record ProcessWebhookCommand(OmadaWebhookPayload Payload) : IRequest<Result>;

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

        var deviceId = await _deviceRepository.GetDeviceByMacAsync(payload.ClientMac);

        if (deviceId is null || deviceId == 0)
        {
            var newDevice = new Device
            {
                MacAddress = payload.ClientMac,
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

         await _deviceSessionRepository.RegisterSessionAsync(session);

        return Result.Success();
    }
}
