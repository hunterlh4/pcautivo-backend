using PCautivoCore.Domain.Models;

namespace PCautivoCore.Domain.Interfaces;

public interface IUserRepository
{
    Task<int> CreateUser(User item);
    Task<bool> UpdateUser(User item);
    Task<User?> GetUserById(int userId);
    Task<User?> GetUserByUsername(string username);
    Task<IEnumerable<User>> GetAllUsers();
    Task<IEnumerable<User>> GetUsersByRoleId(int roleId);
    Task<bool> UpdatePassword(User item);
    Task<bool> CreateUserRole(UserRole item);
    Task<bool> DeleteUserRole(int userId, int roleId);
    Task<UserRole?> GetUserRoleByIds(int userId, int roleId);
    Task<IEnumerable<UserRole>> GetAllUserRoles();
    Task<IEnumerable<UserRole>> GetUserRolesByUserId(int userId);
    Task<bool> CreateUserDetail(UserDetail item);
    Task<int> CreateUserWithDetail(User user, UserDetail detail);
    Task<IEnumerable<User>> GetAllUsersWithDetails();
    Task<IEnumerable<User>> GetUsersByType(int userType);
    Task<UserDetail?> GetUserDetailById(int userId);
    Task<bool> UpdateUserDetail(UserDetail item);
}