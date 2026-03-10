using PCautivoCore.Application.Features.Permissions.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Permissions.Actions;

public class AssignActionPermissionByRoleCommand : IRequest<Result<PermissionByRoleDto>>
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    internal sealed class Handler(IRoleRepository roleRepository, IPermissionRepository permissionRepository) : IRequestHandler<AssignActionPermissionByRoleCommand, Result<PermissionByRoleDto>>
    {
        public async Task<Result<PermissionByRoleDto>> Handle(AssignActionPermissionByRoleCommand request, CancellationToken cancellationToken)
        {
            if (request.RoleId <= 0 || request.PermissionId <= 0)
            {
                return Errors.BadRequest("Datos inválidos.");
            }

            var role = await roleRepository.GetRoleById(request.RoleId);

            if (role == null)
            {
                return Errors.NotFound("Rol no encontrado.");
            }

            var permission = await permissionRepository.GetPermissionById(request.PermissionId);

            if (permission == null)
            {
                return Errors.NotFound("Permiso no encontrado.");
            }

            var rolePermission = await permissionRepository.GetRolePermissionByIds(role.Id, permission.Id);

            if (rolePermission == null)
            {
                await permissionRepository.CreateRolePermission(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTimeOffset.UtcNow   
                });
            }
            else
            {
                await permissionRepository.DeleteRolePermission(rolePermission.RoleId, rolePermission.PermissionId);
            }

            rolePermission = await permissionRepository.GetRolePermissionByIds(role.Id, permission.Id);

            var response = new PermissionByRoleDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Controller = permission.Controller,
                ActionName = permission.ActionName,
                HttpMethod = permission.HttpMethod,
                ActionType = permission.ActionType,
                Status = rolePermission != null
            };

            return response;
        }
    }
}