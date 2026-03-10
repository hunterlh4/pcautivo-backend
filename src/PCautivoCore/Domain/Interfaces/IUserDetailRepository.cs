using PCautivoCore.Domain.Models;

namespace PCautivoCore.Domain.Interfaces;

public interface IUserDetailRepository
{
    Task<UserDetail?> GetUserDetailByUserId(int userId);
}
