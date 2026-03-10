using PCautivoCore.Application.Features.Permissions.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Permissions.Actions;

public class AssignAllPermissionByRoleCommand : IRequest<Result<IEnumerable<PermissionByRoleDto>>>
{
    public int RoleId { get; set; }

    internal sealed class Handler(IRoleRepository roleRepository, IPermissionRepository permissionRepository) : IRequestHandler<AssignAllPermissionByRoleCommand, Result<IEnumerable<PermissionByRoleDto>>>
    {
        public async Task<Result<IEnumerable<PermissionByRoleDto>>> Handle(AssignAllPermissionByRoleCommand request, CancellationToken cancellationToken)
        {
            if (request.RoleId <= 0)
            {
                return Errors.BadRequest("Datos inválidos.");
            }

            var role = await roleRepository.GetRoleById(request.RoleId);

            if (role == null)
            {
                return Errors.NotFound("Rol no encontrado.");
            }

            var permissions = await permissionRepository.GetAllPermissions();
            var rolePermissions = await permissionRepository.GetPermissionsByRoleId(role.Id);
            var rolePermissionIds = rolePermissions.Select(rp => rp.PermissionId).ToHashSet();
            var missingPermissions = permissions.Where(p => !rolePermissionIds.Contains(p.Id)).ToList();

            if (missingPermissions.Count > 0)
            {
                var rolePermissionsToCreate = missingPermissions.Select(p => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = p.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                }).ToList();

                await permissionRepository.CreateManyRolePermission(rolePermissionsToCreate);
            }
            else
            {
                var rolePermissionsToDelete = permissions.Select(p => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = p.Id
                }).ToList();

                await permissionRepository.DeleteManyRolePermission(rolePermissionsToDelete);
            }

            rolePermissions = await permissionRepository.GetPermissionsByRoleId(role.Id);
            rolePermissionIds = rolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            return permissions.Select(p => new PermissionByRoleDto
            {
                Id = p.Id,
                Name = p.Name,
                Controller = p.Controller,
                ActionName = p.ActionName,
                HttpMethod = p.HttpMethod,
                ActionType = p.ActionType,
                Status = rolePermissionIds.Contains(p.Id)
            }).ToList();
        }
    }
}