using PCautivoApi.Shared.Extensions;
using PCautivoApi.Shared.Filters;
using PCautivoApi.Shared.Utils;
using PCautivoCore.Application.Features.Auth.Actions;
using PCautivoCore.Application.Features.Auth.Models;
using PCautivoCore.Application.Features.Permissions.Actions;
using PCautivoCore.Application.Features.Permissions.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PCautivoApi.Controllers;

[Route("api/permissions")]
[ApiController]
[AuthorizeJwt]
public class PermissionController(ISender sender, IActionDescriptorCollectionProvider actionProvider) : ControllerBase
{
    [HttpPost("mapping")]
    public async Task<ActionResult<IEnumerable<MappingPermissionDto>>> MappingPermissions()
    {
        var controller = new ControllerUtil(actionProvider);

        IEnumerable<PermissionModel> permissions = await controller.GetPermissions();

        var response = await sender.Send(new MappingPermissionsCommand
        {
            Permissions = permissions
        });

        return response.ToActionResult();
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<PermissionByRoleDto>>> GetPermissionsByRole([FromQuery] GetPermissionsByRoleQuery query)
    {
        var response = await sender.Send(query);

        return response.ToActionResult();
    }

    [HttpPost("roles/assign-action")]
    public async Task<ActionResult<PermissionByRoleDto>> AssignActionPermissionByRole([FromBody] AssignActionPermissionByRoleCommand command)
    {
        var response = await sender.Send(command);

        return response.ToActionResult();
    }

    [HttpPost("roles/assign-controller")]
    public async Task<ActionResult<IEnumerable<PermissionByRoleDto>>> AssignControllerPermissionByRole([FromBody] AssignControllerPermissionByRoleCommand command)
    {
        var response = await sender.Send(command);

        return response.ToActionResult();
    }

    [HttpPost("roles/assign-all")]
    public async Task<ActionResult<IEnumerable<PermissionByRoleDto>>> AssignAllPermissionByRole([FromBody] AssignAllPermissionByRoleCommand command)
    {
        var response = await sender.Send(command);

        return response.ToActionResult();
    }
}
