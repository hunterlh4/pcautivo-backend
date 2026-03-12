using Dapper;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Infrastructure.Persistence;

namespace PCautivoCore.Infrastructure.Repositories;

public class DeviceRepository(MssqlContext context) : IDeviceRepository
{
    public async Task<int?> GetDeviceByMacAsync(string macAddress)
    {
        using var connection = context.CreateDefaultConnection();
        var query = "SELECT Id FROM Devices WHERE MacAddress = @MacAddress;";
        return await connection.QuerySingleOrDefaultAsync<int?>(query, new { MacAddress = macAddress });
    }

    public async Task<int> AddDeviceAsync(Device device)
    {
        using var connection = context.CreateDefaultConnection();
        var query = @"
            INSERT INTO Devices (MacAddress, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@MacAddress, @CreatedAt)";

        return await connection.QuerySingleAsync<int>(query, device);
    }
}
