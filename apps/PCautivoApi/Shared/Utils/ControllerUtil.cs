using PCautivoApi.Shared.Filters;
using PCautivoCore.Application.Features.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Reflection;

namespace PCautivoApi.Shared.Utils;

public class ControllerUtil
{
    private readonly IActionDescriptorCollectionProvider _actionProvider;

    public ControllerUtil(IActionDescriptorCollectionProvider actionProvider)
    {
        _actionProvider = actionProvider;
    }

    public async Task<IEnumerable<PermissionModel>> GetPermissions()
    {
        List<PermissionModel> permissions = new List<PermissionModel>();

        try
        {
            IEnumerable<PermissionModel> actionsPermissions = GetActionsPermissions();

            if (actionsPermissions.Any())
            {
                permissions.AddRange(actionsPermissions);
            }
        }
        catch (Exception)
        {
            permissions = new List<PermissionModel>();
        }

        return permissions
            .OrderBy(x => x.Controller)
            .OrderBy(x => x.ActionName)
            .ToList();
    }

    public IEnumerable<PermissionModel> GetActionsPermissions()
    {
        try
        {
            var permissions = _actionProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Where(ad =>
                    !ad.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any() &&
                    !ad.EndpointMetadata.OfType<AllowForbiddenAttribute>().Any() &&
                    ad.ControllerTypeInfo.GetCustomAttribute<AllowAnonymousAttribute>() == null &&
                    ad.ControllerTypeInfo.GetCustomAttribute<AllowForbiddenAttribute>() == null &&
                    ad.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.Any() == true)
                .Select(ad =>
                {
                    IEnumerable<string> httpMethods = ad.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.ToList() ?? [];

                    string allowedMethods = string.Empty;

                    if (httpMethods.Any())
                    {
                        allowedMethods = string.Join(",", httpMethods.Select(m => m.ToUpper()));
                    }

                    return new PermissionModel
                    {
                        Name = ad.ActionName,
                        Controller = ad.ControllerName,
                        ActionName = ad.ActionName,
                        HttpMethod = allowedMethods,
                        ActionType = "ACTION"
                    };
                });

            return permissions;
        }
        catch (Exception)
        {
            return new List<PermissionModel>();
        }
    }
}