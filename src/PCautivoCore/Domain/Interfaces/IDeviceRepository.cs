using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PCautivoCore.Domain.Models;

namespace PCautivoCore.Domain.Interfaces;

public interface IDeviceRepository
{

    Task<int?> GetDeviceByMacAsync(string macAddress);

    Task<Dictionary<string, int>> GetDeviceIdsByMacAsync(IEnumerable<string> macAddresses);

    Task<int> AddDeviceAsync(Device device);

    Task<int> AddDevicesAsync(IEnumerable<Device> devices);

    Task<int> UpdateDeviceDniByMacAsync(string macAddress, string dni);
}
