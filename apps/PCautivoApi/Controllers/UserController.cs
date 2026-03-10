using PCautivoApi.Shared.Extensions;
using PCautivoApi.Shared.Filters;
using PCautivoCore.Application.Features.Roles.Dtos;
using PCautivoCore.Application.Features.Users.Actions;
using PCautivoCore.Application.Features.Users.Dtos;
using PCautivoCore.Application.Features.Users.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PCautivoApi.Controllers;

[Route("api/users")]
[ApiController]
[AuthorizeJwt]
public class UserController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsersDto>>> GetUsers([FromQuery] GetUsersQuery query)
    {
        var response = await sender.Send(query);

        return response.ToActionResult();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CreateUserDto>> CreateUser([FromBody] CreateUserCommand command)
    {
        var response = await sender.Send(command);

        return response.ToActionResult();
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<UserDto>> GetUserById([FromRoute] int userId)
    {
        var response = await sender.Send(new GetUserByIdQuery(userId));

        return response.ToActionResult();
    }

    [HttpPut("{userId:int}")]
    public async Task<ActionResult> UpdateUser([FromRoute] int userId, [FromBody] UpdateUserRequest payload)
    {
        var response = await sender.Send(new UpdateUserCommand(userId)
        {
            FirstName = payload.FirstName,
            LastName = payload.LastName,
            Email = payload.Email,
            PhoneNumber = payload.PhoneNumber,
            CountryCode = payload.CountryCode,
        });

        return response.ToActionResult();
    }

   
    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<RoleByUserDto>>> GetRolesByUser([FromQuery] GetRolesByUserQuery query)
    {
        var response = await sender.Send(query);

        return response.ToActionResult();
    }

    [HttpPost("roles/assign")]
    public async Task<ActionResult<RoleByUserDto>> AssignRoleByUser([FromBody] AssignRoleByUserCommand command)
    {
        var response = await sender.Send(command);

        return response.ToActionResult();
    }
}
