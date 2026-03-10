using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace PCautivoApi.Shared.Filters;

public class PermissionRequirement : IAuthorizationRequirement;

public class PermissionHandler(
    IUserRepository userRepository,
    IPermissionRepository permissionRepository,
    IAuthContext authContext,
    IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = authContext.UserId;

        if (userId <= 0)
        {
            context.Fail();
            return;
        }

        var user = await userRepository.GetUserById(userId);

        if (user is null)
        {
            context.Fail();
            return;
        }

        // SuperAdmin siempre tiene acceso total
        if (user.SuperUser)
        {
            context.Succeed(requirement);
            return;
        }

        // Obtener información del endpoint actual
        var httpContext = httpContextAccessor.HttpContext;
        var endpoint = httpContext?.GetEndpoint();
        var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        if (actionDescriptor is null)
        {
            // No es un endpoint de controlador (ej. estático), permitir sin restricción
            context.Succeed(requirement);
            return;
        }

        // Si el endpoint tiene AllowForbidden, permitir sin validar permisos
        var allowForbidden = endpoint?.Metadata.GetMetadata<AllowForbiddenAttribute>();
        if (allowForbidden is not null)
        {
            context.Succeed(requirement);
            return;
        }

        var controllerName = actionDescriptor.ControllerName;           // e.g. "Brand"
        var actionName = actionDescriptor.ActionName;                   // e.g. "GetBrands"
        var httpMethod = httpContext?.Request.Method?.ToUpper() ?? "";  // e.g. "GET"

        // Obtener todos los permisos del usuario a través de sus roles
        var userPermissions = await permissionRepository.GetPermissionsByUserId(userId);

        bool hasPermission = userPermissions.Any(p =>
            string.Equals(p.Controller, controllerName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.ActionName, actionName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.HttpMethod, httpMethod, StringComparison.OrdinalIgnoreCase)
        );

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}