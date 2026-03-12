using PCautivoCore.Domain.Enums;
using PCautivoCore.Domain.Models;

namespace PCautivoCore.Domain.Interfaces;

public interface IDeviceSessionRepository
{
    Task<int> RegisterSessionAsync(DeviceSession session);
}
