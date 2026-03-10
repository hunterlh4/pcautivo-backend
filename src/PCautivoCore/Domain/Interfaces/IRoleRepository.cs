using PCautivoCore.Domain.Models;

namespace PCautivoCore.Domain.Interfaces;

public interface IRoleRepository
{
    Task<int> CreateRole(Role item);
    Task<bool> UpdateRole(Role item);
    Task<IEnumerable<Role>> GetAllRoles();
    Task<Role?> GetRoleById(int itemId);
    Task<IEnumerable<Role>> GetRolesByUserId(int userId);
}