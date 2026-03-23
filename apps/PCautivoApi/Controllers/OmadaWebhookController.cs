using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCautivoApi.Shared.Extensions;
using PCautivoCore.Application.Features.OmadaWebhook.Actions;
using PCautivoCore.Application.Features.OmadaWebhook.Dtos;

namespace PCautivoApi.Controllers;

[Route("api/omadaWebhook")]
[AllowAnonymous]
[ApiController]
public class OmadaWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OmadaWebhookController> _logger;

    public OmadaWebhookController(IMediator mediator, ILogger<OmadaWebhookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("session")]
    [AllowAnonymous] // Importante: Omada no enviará un JWT, así que debe ser anónimo
    public async Task<IActionResult> ReceiveSessionEvent([FromBody] OmadaWebhookPayloadDto payload)
    {

        if (payload == null || string.IsNullOrEmpty(payload.EventType))
        {
            return BadRequest();
        }

        var result = await _mediator.Send(new ProcessWebhookCommand(payload));

        return result.ToActionResult();
    }
}
