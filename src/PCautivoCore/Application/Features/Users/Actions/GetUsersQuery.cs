using PCautivoCore.Application.Features.Users.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Users.Actions;

public class GetUsersQuery : IRequest<Result<IEnumerable<UsersDto>>>
{
    internal sealed class Handler(
        IUserRepository userRepository, 
        IRoleRepository roleRepository,
        IUserPropertyRepository userPropertyRepository
    ) : IRequestHandler<GetUsersQuery, Result<IEnumerable<UsersDto>>>
    {
        public async Task<Result<IEnumerable<UsersDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllUsersWithDetails();
            var roles = await roleRepository.GetAllRoles();
            var userRoles = await userRepository.GetAllUserRoles();
            var propertiesCountByUser = await userPropertyRepository.GetPropertiesCountByUsers();

            return users
                .Where(user => !user.SuperUser)
                .Select(user => new UsersDto
            {
                Id = user.Id,
                Username = user.Username,
                UserType = user.UserType,
                PropertiesCount = propertiesCountByUser.GetValueOrDefault(user.Id, 0),
                Detail = user.Detail != null ? new UsersDetailDto
                {
                    FirstName = user.Detail.FirstName,
                    LastName = user.Detail.LastName,
                    Email = user.Detail.Email,
                    PhoneNumber = user.Detail.PhoneNumber,
                    CountryCode = user.Detail.CountryCode
                } : null,
                Roles = userRoles.Where(ur => ur.UserId == user.Id).Join(roles, ur => ur.RoleId, r => r.Id, (ur, r) => new UsersRoleDto
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            }).ToList();
        }
    }
}