using Dapper;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Infrastructure.Persistence;

namespace PCautivoCore.Infrastructure.Repositories;

public class DeviceSessionRepository(MssqlContext context) : IDeviceSessionRepository
{
    public async Task<HashSet<string>> GetExistingSessionKeysAsync(IEnumerable<DeviceSession> sessions)
    {
        var items = sessions.ToList();
        if (items.Count == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var deviceIds = items.Select(x => x.DeviceId).Distinct().ToArray();
        var sessionIds = items
            .Select(x => x.OmadaId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        using var connection = context.CreateDefaultConnection();
        IEnumerable<DeviceSession> rows;

        if (sessionIds.Length > 0)
        {
            const string bySessionIdQuery = @"
                                SELECT DeviceId, SessionId AS OmadaId, StartTime, EndTime, DurationSeconds
                FROM DeviceSessions
                WHERE SessionId IN @SessionIds
                  AND DeviceId IN @DeviceIds;";

            rows = await connection.QueryAsync<DeviceSession>(bySessionIdQuery, new
            {
                DeviceIds = deviceIds,
                SessionIds = sessionIds
            });
        }
        else
        {
            var startTimes = items.Select(x => x.StartTime).Distinct().ToArray();
            const string byStartQuery = @"
                                SELECT DeviceId, SessionId AS OmadaId, StartTime, EndTime, DurationSeconds
                FROM DeviceSessions
                WHERE DeviceId IN @DeviceIds
                                    AND StartTime IN @StartTimes;";

            rows = await connection.QueryAsync<DeviceSession>(byStartQuery, new
            {
                DeviceIds = deviceIds,
                StartTimes = startTimes
            });
        }

        return rows
            .Select(x => BuildSessionKey(x.DeviceId, x.OmadaId, x.StartTime, x.EndTime))
            .ToHashSet(StringComparer.Ordinal);
    }

    public async Task<int> RegisterSessionAsync(DeviceSession session)
    {
        using var connection = context.CreateDefaultConnection();

        var sessionQuery = @"
            INSERT INTO DeviceSessions (DeviceId, SessionId, StartTime, EndTime, DurationSeconds)
            OUTPUT INSERTED.Id
            VALUES (@DeviceId, @OmadaId, @StartTime, @EndTime, @DurationSeconds);
        ";

        return await connection.QuerySingleAsync<int>(sessionQuery, session);
    }

    public async Task<int> RegisterSessionsAsync(IEnumerable<DeviceSession> sessions)
    {
        var items = sessions as DeviceSession[] ?? sessions.ToArray();
        if (items.Length == 0)
        {
            return 0;
        }

        using var connection = context.CreateDefaultConnection();
        const string query = @"
            INSERT INTO DeviceSessions (DeviceId, SessionId, StartTime, EndTime, DurationSeconds)
            SELECT @DeviceId, @OmadaId, @StartTime, @EndTime, @DurationSeconds
            WHERE NOT EXISTS (
                SELECT 1
                FROM DeviceSessions
                WHERE (
                    @OmadaId IS NOT NULL
                    AND DeviceId = @DeviceId
                    AND SessionId = @OmadaId
                )
                OR (
                    @OmadaId IS NULL
                    AND DeviceId = @DeviceId
                    AND StartTime = @StartTime
                    AND ISNULL(EndTime, '19000101') = ISNULL(@EndTime, '19000101')
                )
            );";

        return await connection.ExecuteAsync(query, items);
    }

    private static string BuildSessionKey(int deviceId, string? omadaId, DateTime startTime, DateTime? endTime)
    {
        if (!string.IsNullOrWhiteSpace(omadaId))
        {
            return $"{deviceId}|OID:{omadaId}";
        }

        return $"{deviceId}|{startTime.Ticks}|{endTime?.Ticks ?? 0}";
    }
}
