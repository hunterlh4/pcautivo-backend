using PCautivoCore.Application.Features.Auth.Models;
using PCautivoCore.Application.Features.Permissions.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Domain.Models;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.Auth.Actions;

public record MappingPermissionsCommand : IRequest<Result<IEnumerable<MappingPermissionDto>>>
{
    public IEnumerable<PermissionModel> Permissions { get; init; } = [];

    internal sealed class Handler(IPermissionRepository permissionRepository) : IRequestHandler<MappingPermissionsCommand, Result<IEnumerable<MappingPermissionDto>>>
    {
        public async Task<Result<IEnumerable<MappingPermissionDto>>> Handle(MappingPermissionsCommand request, CancellationToken cancellationToken)
        {
            var requestedPerms = request.Permissions.Select(x => new Permission
            {
                Name = x.Name,
                Controller = x.Controller,
                ActionName = x.ActionName,
                HttpMethod = x.HttpMethod,
                ActionType = x.ActionType
            });

            var existingPerms = await permissionRepository.GetAllPermissions();

            var codeItemsDictionary = requestedPerms.ToDictionary(x => (x.Controller, x.ActionName, x.HttpMethod));
            var dbItemsDictionary = existingPerms.ToDictionary(x => (x.Controller, x.ActionName, x.HttpMethod));

            var actionsToAdd = requestedPerms.Where(x => !dbItemsDictionary.ContainsKey((x.Controller, x.ActionName, x.HttpMethod))).ToList();
            var actionsToUpdate = existingPerms.Where(x => codeItemsDictionary.ContainsKey((x.Controller, x.ActionName, x.HttpMethod))).ToList();
            var actionsToRemove = existingPerms.Where(x => !codeItemsDictionary.ContainsKey((x.Controller, x.ActionName, x.HttpMethod))).ToList();

            if (actionsToAdd.Count > 0)
            {
                actionsToAdd.ForEach(x => x.CreatedAt = DateTimeOffset.UtcNow);

                await permissionRepository.CreateManyPermissions(actionsToAdd);
            }

            if (actionsToUpdate.Count > 0)
            {
                actionsToUpdate.ForEach(x => x.UpdatedAt = DateTimeOffset.UtcNow);

                await permissionRepository.UpdateManyPermissions(actionsToUpdate);
            }

            if (actionsToRemove.Count > 0)
            {
                foreach (var action in actionsToRemove)
                {
                    await permissionRepository.DeletePermissionById(action.Id);
                }
            }

            var currentPerms = await permissionRepository.GetAllPermissions();

            var records = currentPerms.Select(x => new MappingPermissionDto
            {
                Id = x.Id,
                Name = x.Name,
                Controller = x.Controller,
                ActionName = x.ActionName,
                HttpMethod = x.HttpMethod,
                ActionType = x.ActionType
            }).ToList();

            return records;
        }
    }
}