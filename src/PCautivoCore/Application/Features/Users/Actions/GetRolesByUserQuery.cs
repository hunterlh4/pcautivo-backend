using PCautivoCore.Application.Features.Roles.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Users.Actions;

public class GetRolesByUserQuery : IRequest<Result<IEnumerable<RoleByUserDto>>>
{
    public int? UserId { get; set; }

    internal sealed class Handler(IUserRepository userRepository, IRoleRepository roleRepository) : IRequestHandler<GetRolesByUserQuery, Result<IEnumerable<RoleByUserDto>>>
    {
        public async Task<Result<IEnumerable<RoleByUserDto>>> Handle(GetRolesByUserQuery request, CancellationToken cancellationToken)
        {
            var roles = await roleRepository.GetAllRoles();
            var userRoles = request.UserId.HasValue ? await userRepository.GetUserRolesByUserId(request.UserId.Value) : Enumerable.Empty<UserRole>();

            var userRoleIds = userRoles.Select(rp => rp.RoleId).ToHashSet();

            return roles.Select(x => new RoleByUserDto
            {
                Id = x.Id,
                Name = x.Name,
                Status = request.UserId.HasValue && userRoleIds.Contains(x.Id)
            }).ToList();
        }
    }
}