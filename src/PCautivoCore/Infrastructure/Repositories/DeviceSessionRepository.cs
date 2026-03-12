using Dapper;
using PCautivoCore.Domain.Enums;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Infrastructure.Persistence;

namespace PCautivoCore.Infrastructure.Repositories;

public class DeviceSessionRepository(MssqlContext context) : IDeviceSessionRepository
{
    public async Task<int> RegisterSessionAsync(DeviceSession session)
    {
        using var connection = context.CreateDefaultConnection();

        // Registrar la sesión (ENTRADA o SALIDA) y retornar el Id generado
        var sessionQuery = @"
            INSERT INTO DeviceSessions (DeviceId, SessionType, EventTime)
            OUTPUT INSERTED.Id
            VALUES (@DeviceId, @SessionType, @EventTime);
        ";

        return await connection.QuerySingleAsync<int>(sessionQuery, session);
    }
}
