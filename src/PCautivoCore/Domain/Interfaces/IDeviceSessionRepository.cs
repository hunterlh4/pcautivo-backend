using PCautivoCore.Domain.Models;
using System.Collections.Generic;

namespace PCautivoCore.Domain.Interfaces;

public interface IDeviceSessionRepository
{
    Task<int> RegisterSessionAsync(DeviceSession session);
    Task<int> RegisterSessionsAsync(IEnumerable<DeviceSession> sessions);
    Task<HashSet<string>> GetExistingSessionKeysAsync(IEnumerable<DeviceSession> sessions);
}
