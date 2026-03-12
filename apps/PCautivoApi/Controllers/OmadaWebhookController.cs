using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PCautivoApi.Shared.Extensions;
using PCautivoCore.Application.Features.Omada.Actions;
using PCautivoCore.Infrastructure.Models.Omada;
using PCautivoCore.Shared.Responses;

namespace PCautivoApi.Controllers;

[Route("api/webhooks/omada")]
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
    public async Task<IActionResult> ReceiveSessionEvent([FromBody] OmadaWebhookPayload payload)
    {
        _logger.LogWarning("=> Webhook recibido en el endpoint. EventType: {EventType}, ClientMac: {ClientMac}", payload?.EventType, payload?.ClientMac);

        if (payload == null || string.IsNullOrEmpty(payload.EventType))
        {
            _logger.LogWarning("=> El payload del webhook llegó nulo o sin EventType.");
            return BadRequest();
        }

        var result = await _mediator.Send(new ProcessWebhookCommand(payload));

        return result.ToActionResult();
    }
}
