using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCautivoCore.Application.Features.Omada.Actions;
using PCautivoCore.Application.Features.Omada.Dtos;
using PCautivoApi.Shared.Extensions;

namespace PCautivoApi.Controllers;

[Route("api/omada")]
[AllowAnonymous]
[ApiController]
public class OmadaController(ISender sender) : ControllerBase
{
    [HttpPost("session")]
    public async Task<ActionResult<OmadaSessionSyncDto>> SyncSessions()
    {
        var response = await sender.Send(new SyncOmadaSessionsCommand());

        return response.ToActionResult();
    }
}
