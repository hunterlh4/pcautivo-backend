using PCautivoApi.Shared.Extensions;
using PCautivoApi.Shared.Filters;
using PCautivoCore.Application.Features.Roles.Actions;
using PCautivoCore.Application.Features.Roles.Dtos;
using PCautivoCore.Application.Features.Roles.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PCautivoApi.Controllers;

[Route("api/roles")]
[ApiController]
[AuthorizeJwt]
public class RoleController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles([FromQuery] GetRolesQuery query)
    {
        var response = await sender.Send(query);

        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<CreateRoleDto>> CreateRole([FromBody] CreateRoleCommand command)
    {
        var response = await sender.Send(command);

        return response.ToActionResult();
    }

    [HttpGet("{roleId:int}")]
    public async Task<ActionResult<RoleDto>> GetRoleById([FromRoute] int roleId)
    {
        var response = await sender.Send(new GetRoleByIdQuery(roleId));

        return response.ToActionResult();
    }

    [HttpPut("{roleId:int}")]
    public async Task<ActionResult> UpdateRole([FromRoute] int roleId, [FromBody] UpdateRoleRequest payload)
    {
        var response = await sender.Send(new UpdateRoleCommand(roleId)
        {
            Name = payload.Name
        });

        return response.ToActionResult();
    }
}
