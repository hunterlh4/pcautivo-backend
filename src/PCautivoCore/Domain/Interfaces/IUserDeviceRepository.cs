using PCautivoCore.Domain.Models;

namespace PCautivoCore.Domain.Interfaces;

public interface IUserDeviceRepository
{
    Task<int> AssignUsersToDevicesAsync(IEnumerable<UserDevice> userDevices);
}
