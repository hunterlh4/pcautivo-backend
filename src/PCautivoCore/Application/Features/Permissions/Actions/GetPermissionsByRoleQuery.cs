using PCautivoCore.Application.Features.Permissions.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Permissions.Actions;

public class GetPermissionsByRoleQuery : IRequest<Result<IEnumerable<PermissionByRoleDto>>>
{
    public int? RoleId { get; set; }

    internal sealed class Handler(IPermissionRepository permissionRepository) : IRequestHandler<GetPermissionsByRoleQuery, Result<IEnumerable<PermissionByRoleDto>>>
    {
        public async Task<Result<IEnumerable<PermissionByRoleDto>>> Handle(GetPermissionsByRoleQuery request, CancellationToken cancellationToken)
        {
            var permissions = await permissionRepository.GetAllPermissions();
            var rolePermissions = request.RoleId.HasValue ? await permissionRepository.GetPermissionsByRoleId(request.RoleId.Value) : [];

            var rolePermissionIds = rolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            return permissions.Select(x => new PermissionByRoleDto
            {
                Id = x.Id,
                Name = x.Name,
                Controller = x.Controller,
                ActionName = x.ActionName,
                HttpMethod = x.HttpMethod,
                ActionType = x.ActionType,
                Status = request.RoleId.HasValue && rolePermissionIds.Contains(x.Id)
            }).ToList();
        }
    }
}