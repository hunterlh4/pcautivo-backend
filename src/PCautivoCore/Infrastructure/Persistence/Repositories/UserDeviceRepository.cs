using Dapper;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;

namespace PCautivoCore.Infrastructure.Persistence.Repositories;

public class UserDeviceRepository(MssqlContext context) : IUserDeviceRepository
{
    public async Task<int> AssignUsersToDevicesAsync(IEnumerable<UserDevice> userDevices)
    {
        var items = userDevices as UserDevice[] ?? userDevices.ToArray();
        if (items.Length == 0)
        {
            return 0;
        }

        using var db = context.CreateDefaultConnection();

        const string sql = @"
            INSERT INTO UserDevices (UserId, DeviceId, CreatedAt)
            SELECT @UserId, @DeviceId, @CreatedAt
            WHERE NOT EXISTS (
                SELECT 1
                FROM UserDevices
                WHERE UserId = @UserId
                  AND DeviceId = @DeviceId
            );";

        return await db.ExecuteAsync(sql, items);
    }
}
