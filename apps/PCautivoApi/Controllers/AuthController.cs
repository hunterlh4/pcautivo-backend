using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCautivoApi.Shared.Extensions;
using PCautivoApi.Shared.Filters;
using PCautivoCore.Application.Features.Auth.Actions;
using PCautivoCore.Application.Features.Auth.Dtos;
using PCautivoCore.Application.Features.CaptiveAuth.Actions;
using PCautivoCore.Application.Features.CaptiveAuth.Dtos;

namespace PCautivoApi.Controllers;

[Route("api/auth")]
[ApiController]
[AuthorizeJwt]
public class AuthController(ISender sender) : ControllerBase
{
    //[HttpPost("login")]
    //[AllowAnonymous]
    //public async Task<ActionResult<LoginDto>> Login([FromBody] LoginCommand command)
    //{
    //    var response = await sender.Send(command);

    //    return response.ToActionResult();
    //}

    /// <summary>
    /// Autentica un usuario del portal cautivo, autoriza su MAC en Omada (portal/auth, authType=5)
    /// y retorna el JWT junto con la URL de landing.
    /// </summary>
    /// <param name="command">Credenciales del usuario y datos del dispositivo (clientMac, apMac, ssidName, radioId, originUrl).</param>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<CaptiveLoginDto>> Login([FromBody] CaptiveLoginCommand command)
    {
        var result = await sender.Send(command);
        return result.ToActionResult();
    }

    [HttpGet("me")]
    [AllowForbidden]
    public async Task<ActionResult<AuthMeDto>> Me([FromQuery] AuthMeQuery query)
    {
        var response = await sender.Send(query);

        return response.ToActionResult();
    }

    [HttpGet("profile")]
    [AllowForbidden]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var response = await sender.Send(new GetProfileQuery());

        return response.ToActionResult();
    }

    [HttpPut("profile")]
    [AllowForbidden]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var response = await sender.Send(command);

        return response.ToActionResult();
    }

    [HttpPut("change-password")]
    [AllowForbidden]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var response = await sender.Send(command);

        return response.ToActionResult();
    }
}
