using System;
using System.Threading.Tasks;
using PCautivoCore.Domain.Models;

namespace PCautivoCore.Domain.Interfaces;

public interface IDeviceRepository
{

    Task<int?> GetDeviceByMacAsync(string macAddress);


    Task<int> AddDeviceAsync(Device device);
}
