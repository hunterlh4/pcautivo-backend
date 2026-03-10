using PCautivoCore.Application.Features.Roles.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Roles.Actions;

public record GetRolesQuery : IRequest<Result<IEnumerable<RoleDto>>>
{
    internal sealed class Handler(IRoleRepository roleRepository) : IRequestHandler<GetRolesQuery, Result<IEnumerable<RoleDto>>>
    {
        public async Task<Result<IEnumerable<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var items = await roleRepository.GetAllRoles();

            return items.Select(x => new RoleDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }
    }
}