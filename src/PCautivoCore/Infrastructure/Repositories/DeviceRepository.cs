using Dapper;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Infrastructure.Persistence;

namespace PCautivoCore.Infrastructure.Repositories;

public class DeviceRepository(MssqlContext context) : IDeviceRepository
{
    private sealed class DeviceLookupRow
    {
        public int Id { get; set; }
        public string MacAddress { get; set; } = string.Empty;
    }

    private static string NormalizeMac(string macAddress)
    {
        return macAddress
            .Trim()
            .Replace(':', '-')
            .Replace('.', '-')
            .ToUpperInvariant();
    }

    private static string NormalizeMacForCompare(string macAddress)
    {
        return NormalizeMac(macAddress).Replace("-", string.Empty);
    }

    public async Task<int?> GetDeviceByMacAsync(string macAddress)
    {
        using var connection = context.CreateDefaultConnection();
        const string query = @"
            SELECT TOP 1 Id
            FROM Devices
            WHERE REPLACE(REPLACE(UPPER(MacAddress), '-', ''), ':', '') = @NormalizedMac;";

        return await connection.QuerySingleOrDefaultAsync<int?>(query, new
        {
            NormalizedMac = NormalizeMacForCompare(macAddress)
        });
    }

    public async Task<Dictionary<string, int>> GetDeviceIdsByMacAsync(IEnumerable<string> macAddresses)
    {
        var normalizedInput = macAddresses
            .Select(NormalizeMac)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedInput.Length == 0)
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        var normalizedLookup = normalizedInput
            .Select(NormalizeMacForCompare)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        using var connection = context.CreateDefaultConnection();
        const string query = @"
            SELECT Id, MacAddress
            FROM Devices
            WHERE REPLACE(REPLACE(UPPER(MacAddress), '-', ''), ':', '') IN @NormalizedMacs;";

        var rows = await connection.QueryAsync<DeviceLookupRow>(query, new { NormalizedMacs = normalizedLookup });
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            map[NormalizeMac(row.MacAddress)] = row.Id;
        }

        return map;
    }

    public async Task<int> AddDeviceAsync(Device device)
    {
        using var connection = context.CreateDefaultConnection();
        const string query = @"
            INSERT INTO Devices (MacAddress, Dni, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@MacAddress, @Dni, @CreatedAt)";

        return await connection.QuerySingleAsync<int>(query, new
        {
            MacAddress = NormalizeMac(device.MacAddress),
            device.Dni,
            device.CreatedAt
        });
    }

    public async Task<int> AddDevicesAsync(IEnumerable<Device> devices)
    {
        var items = devices
            .Select(x => new
            {
                MacAddress = NormalizeMac(x.MacAddress),
                x.Dni,
                x.CreatedAt
            })
            .ToArray();

        if (items.Length == 0)
        {
            return 0;
        }

        using var connection = context.CreateDefaultConnection();
        const string query = @"
            INSERT INTO Devices (MacAddress, Dni, CreatedAt)
            SELECT @MacAddress, @Dni, @CreatedAt
            WHERE NOT EXISTS (
                SELECT 1
                FROM Devices
                WHERE REPLACE(REPLACE(UPPER(MacAddress), '-', ''), ':', '') = REPLACE(REPLACE(UPPER(@MacAddress), '-', ''), ':', '')
            );";

        return await connection.ExecuteAsync(query, items);
    }

    public async Task<int> UpdateDeviceDniByMacAsync(string macAddress, string dni)
    {
        using var connection = context.CreateDefaultConnection();
        const string query = @"
            UPDATE Devices
            SET Dni = @Dni
            WHERE REPLACE(REPLACE(UPPER(MacAddress), '-', ''), ':', '') = @NormalizedMac
              AND (Dni IS NULL OR Dni <> @Dni);";

        return await connection.ExecuteAsync(query, new
        {
            NormalizedMac = NormalizeMacForCompare(macAddress),
            Dni = dni
        });
    }
}
