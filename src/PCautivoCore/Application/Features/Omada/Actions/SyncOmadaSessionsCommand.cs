using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCautivoCore.Application.Features.Omada.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Infrastructure.Models.Omada;
using PCautivoCore.Infrastructure.Settings;
using PCautivoCore.Shared.Responses;
using System.Globalization;

namespace PCautivoCore.Application.Features.Omada.Actions;

public record SyncOmadaSessionsCommand : IRequest<Result<OmadaSessionSyncDto>>
{
    public sealed class Validator : AbstractValidator<SyncOmadaSessionsCommand>
    {
        public Validator()
        {
            RuleFor(x => x).NotNull();
        }
    }

    internal sealed class Handler : IRequestHandler<SyncOmadaSessionsCommand, Result<OmadaSessionSyncDto>>
    {
        private const int PageSize = 200;
        private readonly IOmadaService omadaService;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDeviceSessionRepository deviceSessionRepository;
        private readonly IOptions<OmadaSettings> omadaOptions;
        private readonly IOptions<OmadaSyncJobSettings> jobOptions;

        public Handler(
            IOmadaService omadaService,
            IDeviceRepository deviceRepository,
            IDeviceSessionRepository deviceSessionRepository,
            IOptions<OmadaSettings> omadaOptions,
            IOptions<OmadaSyncJobSettings> jobOptions)
        {
            this.omadaService = omadaService;
            this.deviceRepository = deviceRepository;
            this.deviceSessionRepository = deviceSessionRepository;
            this.omadaOptions = omadaOptions;
            this.jobOptions = jobOptions;
        }

        public async Task<Result<OmadaSessionSyncDto>> Handle(SyncOmadaSessionsCommand command, CancellationToken cancellationToken)
        {
            var rangeTime = ParseRangeTime(jobOptions.Value.RangeTime);
            var timeZoneId = string.IsNullOrWhiteSpace(jobOptions.Value.TimeZoneId)
                ? "SA Pacific Standard Time"
                : jobOptions.Value.TimeZoneId;

            var window = BuildWindow(DateTimeOffset.UtcNow, rangeTime, timeZoneId);
            var timeStartMs = new DateTimeOffset(window.startUtc).ToUnixTimeMilliseconds();
            var timeEndMs = new DateTimeOffset(window.endUtc).ToUnixTimeMilliseconds();

            if (timeStartMs >= timeEndMs) return Errors.BadRequest("Rango de fechas invalido para sincronizacion.");

            var siteId = omadaOptions.Value.Site;
            if (string.IsNullOrWhiteSpace(siteId)) return Errors.BadRequest("SiteId es requerido.");

            var sessions = await omadaService.GetHotspotClientSessionsAsync(
                siteId,
                timeStartMs,
                timeEndMs,
                PageSize,
                cancellationToken);

            var candidates = BuildCandidateSessions(sessions);
            if (candidates.Count == 0)
            {
                return new OmadaSessionSyncDto
                {
                    CreatedDevices = 0,
                    InsertedEvents = 0
                };
            }

            var macAddresses = candidates
                .Select(x => x.ClientMac)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var deviceMap = await deviceRepository.GetDeviceIdsByMacAsync(macAddresses);

            var missingMacs = macAddresses
                .Where(mac => !deviceMap.ContainsKey(mac))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var createdDevices = 0;

            if (missingMacs.Length > 0)
            {
                var devicesToInsert = missingMacs
                    .Select(mac => new Device
                    {
                        MacAddress = mac,
                        Dni = null,
                        CreatedAt = NormalizeForPersistence(DateTime.UtcNow)
                    })
                    .ToArray();

                createdDevices = await deviceRepository.AddDevicesAsync(devicesToInsert);

                var newMap = await deviceRepository.GetDeviceIdsByMacAsync(missingMacs);
                foreach (var pair in newMap)
                {
                    deviceMap[pair.Key] = pair.Value;
                }
            }

            var sessionsToValidate = candidates
                .Where(x => deviceMap.ContainsKey(x.ClientMac))
                .Select(x => new DeviceSession
                {
                    DeviceId = deviceMap[x.ClientMac],
                    OmadaId = x.OmadaId,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    DurationSeconds = x.DurationSeconds,
                })
                .ToList();

            var existingKeys = await deviceSessionRepository.GetExistingSessionKeysAsync(sessionsToValidate);

            var sessionsToInsert = sessionsToValidate
                .DistinctBy(BuildSessionKey)
                .Where(x => !existingKeys.Contains(BuildSessionKey(x)))
                .ToList();

            var insertedEvents = await deviceSessionRepository.RegisterSessionsAsync(sessionsToInsert);

            return new OmadaSessionSyncDto
            {
                CreatedDevices = createdDevices,
                InsertedEvents = insertedEvents
            };
        }

        private static List<CandidateSession> BuildCandidateSessions(IEnumerable<OmadaHotspotClientSession> sessions)
        {
            var candidates = new List<CandidateSession>();

            foreach (var item in sessions)
            {
                if (string.IsNullOrWhiteSpace(item.ClientMac) ||
                    string.IsNullOrWhiteSpace(item.Id) ||
                    !item.StartTime.HasValue)
                {
                    continue;
                }

                var normalizedMac = NormalizeMacAddress(item.ClientMac);
                var normalizedOmadaId = NormalizeOmadaId(item.Id);
                var startTime = NormalizeForPersistence(item.StartTime.Value);
                DateTime? endTime = null;

                if (item.EndTime.HasValue)
                {
                    endTime = NormalizeForPersistence(item.EndTime.Value);
                }
                else if (item.DurationSeconds > 0)
                {
                    endTime = NormalizeForPersistence(item.StartTime.Value.AddSeconds(item.DurationSeconds));
                }

                candidates.Add(new CandidateSession(
                    normalizedMac,
                    normalizedOmadaId,
                    startTime,
                    endTime,
                    item.DurationSeconds));
            }

            return candidates
                .DistinctBy(x => BuildCandidateKey(x.ClientMac, x.OmadaId, x.StartTime, x.EndTime))
                .ToList();
        }

        private static string BuildCandidateKey(string clientMac, string? omadaId, DateTime startTimeUtc, DateTime? endTimeUtc)
        {
            if (!string.IsNullOrWhiteSpace(omadaId))
            {
                return $"{clientMac}|OID:{omadaId}";
            }

            return $"{clientMac}|{startTimeUtc.Ticks}|{endTimeUtc?.Ticks ?? 0}";
        }

        private static (DateTime startUtc, DateTime endUtc) BuildWindow(DateTimeOffset runAt, TimeOnly anchorTime, string timeZoneId)
        {
            var tz = ResolveTimeZone(timeZoneId);
            var runUtc = runAt.UtcDateTime;
            var runLocal = TimeZoneInfo.ConvertTimeFromUtc(runUtc, tz);

            var anchorLocal = new DateTime(
                runLocal.Year,
                runLocal.Month,
                runLocal.Day,
            anchorTime.Hour,
            anchorTime.Minute,
                0,
                DateTimeKind.Unspecified);

            var endLocal = runLocal >= anchorLocal ? anchorLocal : anchorLocal.AddDays(-1);
            var startLocal = endLocal.AddDays(-1);

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);

            return (startUtc, endUtc);
        }

        private static TimeOnly ParseRangeTime(string? rangeTime)
        {
            if (TimeOnly.TryParseExact(rangeTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed;
            }

            return new TimeOnly(8, 0);
        }

        private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
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

        private static string? NormalizeOmadaId(string? omadaId)
        {
            if (string.IsNullOrWhiteSpace(omadaId))
            {
                return null;
            }

            return omadaId.Trim();
        }

        private static string NormalizeMacAddress(string macAddress)
        {
            return macAddress
                .Trim()
                .Replace(':', '-')
                .Replace('.', '-')
                .ToUpperInvariant();
        }

        private static string BuildSessionKey(DeviceSession session)
        {
            if (!string.IsNullOrWhiteSpace(session.OmadaId))
            {
                return $"{session.DeviceId}|OID:{session.OmadaId}";
            }

            return $"{session.DeviceId}|{session.StartTime.Ticks}|{session.EndTime?.Ticks ?? 0}";
        }

        private readonly record struct CandidateSession(
            string ClientMac,
            string? OmadaId,
            DateTime StartTime,
            DateTime? EndTime,
            int DurationSeconds);
    }
}
