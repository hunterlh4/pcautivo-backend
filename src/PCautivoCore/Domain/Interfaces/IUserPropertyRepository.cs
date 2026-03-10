using PCautivoCore.Application.Features.UserProperties.Dtos;
using PCautivoCore.Domain.Models;

namespace PCautivoCore.Domain.Interfaces;

public interface IUserPropertyRepository
{
    Task<bool> AssignUserToProperty(int userId, int propertyId);
    Task<bool> AssignUserToProperties(int userId, List<int> propertyIds);
    Task<bool> RemoveUserFromProperty(int userId, int propertyId);
    Task<bool> RemoveUserFromProperties(int userId, List<int> propertyIds);
    Task<UserProperty?> GetUserProperty(int userId, int propertyId);
    Task<IEnumerable<Property>> GetUserProperties(int userId);
    Task<IEnumerable<UserPropertyDto>> GetUserPropertiesWithDetails(int userId);
    Task<Dictionary<int, int>> GetPropertiesCountByUsers();
}
