using PCautivoApi.Shared.Extensions;
using PCautivoApi.Shared.Filters;
using PCautivoCore.Application.Features.UserProperties.Actions;
using PCautivoCore.Application.Features.UserProperties.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PCautivoApi.Controllers;

[Route("api/user-properties")]
[ApiController]
[AuthorizeJwt]
public class UserPropertyController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> AssignPropertiesToUser([FromBody] AssignPropertiesToUserCommand command)
    {
        var response = await sender.Send(command);

        return response.ToActionResult();
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<IEnumerable<UserPropertyDto>>> GetUserProperties([FromRoute] int userId)
    {
        var response = await sender.Send(new GetUserPropertiesQuery(userId));

        return response.ToActionResult();
    }
}
