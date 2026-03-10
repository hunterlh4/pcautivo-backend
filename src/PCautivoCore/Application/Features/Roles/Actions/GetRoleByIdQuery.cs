using PCautivoCore.Application.Features.Roles.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Roles.Actions;

public record GetRoleByIdQuery(int RoleId) : IRequest<Result<RoleDto>>
{
    internal sealed class Handler(IRoleRepository roleRepository) : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
    {
        public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.RoleId <= 0)
            {
                return Errors.BadRequest("Datos inválidos.");
            }

            var item = await roleRepository.GetRoleById(request.RoleId);

            if (item == null)
            {
                return Errors.NotFound("Rol no encontrado.");
            }

            return new RoleDto
            {
                Id = item.Id,
                Name = item.Name
            };
        }
    }
}