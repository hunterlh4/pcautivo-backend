using PCautivoCore.Domain.Enums;
using PCautivoCore.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace PCautivoApi.Shared.Filters;

/// <summary>
/// Atributo de autorización que bloquea el acceso a usuarios con ciertos UserTypes.
/// Funciona como una lista negra - los UserTypes especificados NO pueden acceder.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ForbidUserTypeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly UserType[] _forbiddenUserTypes;

    public ForbidUserTypeAttribute(params UserType[] forbiddenUserTypes)
    {
        _forbiddenUserTypes = forbiddenUserTypes;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userRepository = context.HttpContext.RequestServices.GetService<IUserRepository>();

        if (userRepository == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Unauthorized" ?? "Unauthorized access." });
            return;
        }

        var user = await userRepository.GetUserById(userId);
        if (user == null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "UserNotFound" ?? "User not found." });
            return;
        }

        if (_forbiddenUserTypes.Contains(user.UserType))
        {
            context.Result = new ObjectResult(new { message = "Forbidden" ?? "You do not have permission to perform this action." })
            {
                StatusCode = 403
            };
            return;
        }
    }
}
