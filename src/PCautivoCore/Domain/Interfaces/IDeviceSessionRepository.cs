using PCautivoCore.Domain.Enums;
using PCautivoCore.Domain.Models;
using System.Collections.Generic;

namespace PCautivoCore.Domain.Interfaces;

public interface IDeviceSessionRepository
{
    Task<int> RegisterSessionAsync(DeviceSession session);
    Task<int> RegisterSessionsAsync(IEnumerable<DeviceSession> sessions);
    Task<bool> ExistsSessionAsync(int deviceId, DeviceSessionType sessionType, DateTime eventTime);
    Task<HashSet<string>> GetExistingSessionKeysAsync(IEnumerable<DeviceSession> sessions);
}
