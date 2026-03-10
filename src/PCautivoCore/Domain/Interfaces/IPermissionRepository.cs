using PCautivoCore.Domain.Models;

namespace PCautivoCore.Domain.Interfaces;

public interface IPermissionRepository
{
    Task<bool> CreateManyPermissions(IEnumerable<Permission> items);
    Task<bool> UpdateManyPermissions(IEnumerable<Permission> items);
    Task<bool> DeletePermissionById(int itemId);
    Task<Permission?> GetPermissionById(int itemId);
    Task<IEnumerable<Permission>> GetAllPermissions();
    Task<IEnumerable<Permission>> GetPermissionsByUserId(int userId);
    Task<bool> CreateRolePermission(RolePermission item);
    Task<bool> DeleteRolePermission(int roleId, int permissionId);
    Task<bool> DeleteRolePermissionByRole(int roleId);
    Task<bool> CreateManyRolePermission(IEnumerable<RolePermission> items);
    Task<bool> DeleteManyRolePermission(IEnumerable<RolePermission> items);
    Task<RolePermission?> GetRolePermissionByIds(int roleId, int permissionId);
    Task<IEnumerable<RolePermission>> GetPermissionsByRoleId(int roleId);
}