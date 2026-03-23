using Dapper;
using PCautivoCore.Domain.Enums;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Infrastructure.Persistence;

namespace PCautivoCore.Infrastructure.Repositories;

public class DeviceSessionRepository(MssqlContext context) : IDeviceSessionRepository
{
    private sealed class SessionRow
    {
        public int DeviceId { get; set; }
        public int SessionType { get; set; }
        public string? OmadaId { get; set; }
        public DateTime EventTime { get; set; }
    }

    public async Task<bool> ExistsSessionAsync(int deviceId, DeviceSessionType sessionType, DateTime eventTime)
    {
        using var connection = context.CreateDefaultConnection();

        const string query = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM DeviceSessions
                WHERE DeviceId = @DeviceId
                  AND SessionType = @SessionType
                  AND EventTime = @EventTime
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;";

        return await connection.QuerySingleAsync<bool>(query, new
        {
            DeviceId = deviceId,
            SessionType = sessionType,
            EventTime = eventTime
        });
    }

    public async Task<HashSet<string>> GetExistingSessionKeysAsync(IEnumerable<DeviceSession> sessions)
    {
        var items = sessions.ToList();
        if (items.Count == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var deviceIds = items.Select(x => x.DeviceId).Distinct().ToArray();
        var sessionTypes = items.Select(x => (int)x.SessionType).Distinct().ToArray();
        var sessionIds = items
            .Select(x => x.OmadaId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (sessionIds.Length == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        using var connection = context.CreateDefaultConnection();
        const string query = @"
            SELECT DeviceId, SessionType, SessionId AS OmadaId, EventTime
            FROM DeviceSessions
            WHERE SessionId IN @SessionIds
              AND DeviceId IN @DeviceIds
              AND SessionType IN @SessionTypes;";

        var rows = await connection.QueryAsync<SessionRow>(query, new
        {
            DeviceIds = deviceIds,
            SessionIds = sessionIds,
            SessionTypes = sessionTypes
        });

        return rows
            .Select(x => BuildSessionKey(x.DeviceId, (DeviceSessionType)x.SessionType, x.OmadaId, x.EventTime))
            .ToHashSet(StringComparer.Ordinal);
    }

    public async Task<int> RegisterSessionAsync(DeviceSession session)
    {
        using var connection = context.CreateDefaultConnection();

        // Registrar la sesión (ENTRADA o SALIDA) y retornar el Id generado
        var sessionQuery = @"
            INSERT INTO DeviceSessions (DeviceId, SessionType, SessionId, EventTime)
            OUTPUT INSERTED.Id
            VALUES (@DeviceId, @SessionType, @OmadaId, @EventTime);
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
            INSERT INTO DeviceSessions (DeviceId, SessionType, SessionId, EventTime)
            SELECT @DeviceId, @SessionType, @OmadaId, @EventTime
            WHERE NOT EXISTS (
                SELECT 1
                FROM DeviceSessions
                WHERE (
                    @OmadaId IS NOT NULL
                    AND DeviceId = @DeviceId
                    AND SessionType = @SessionType
                    AND SessionId = @OmadaId
                )
                OR (
                    @OmadaId IS NULL
                    AND DeviceId = @DeviceId
                    AND SessionType = @SessionType
                    AND EventTime = @EventTime
                )
            );";

        return await connection.ExecuteAsync(query, items);
    }

    private static string BuildSessionKey(int deviceId, DeviceSessionType sessionType, string? omadaId, DateTime eventTime)
    {
        if (!string.IsNullOrWhiteSpace(omadaId))
        {
            return $"{deviceId}|{(int)sessionType}|OID:{omadaId}";
        }

        return $"{deviceId}|{(int)sessionType}|{eventTime.Ticks}";
    }
}
