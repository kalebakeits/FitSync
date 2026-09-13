namespace FitSync.Api.Features.Wahoo.Webhook.Controllers;

using FitSync.Api.Configurations;
using FitSync.Api.Features.Wahoo.Webhook.DTOs;
using FitSync.Api.Features.Wahoo.Webhook.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("wahoo")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public class WahooWebhookController(
    IWahooWebhookService webhookService,
    IOptions<WahooOptions> wahooOptions,
    ILogger<WahooWebhookController> logger
) : ControllerBase
{
    private readonly IWahooWebhookService webhookService = webhookService;
    private readonly IOptions<WahooOptions> wahooOptions = wahooOptions;
    private readonly ILogger<WahooWebhookController> logger = logger;

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook(
        [FromBody] WahooWebhookPayload payload,
        CancellationToken cancellationToken
    )
    {
        if (payload.WebhookToken != this.wahooOptions.Value.WebhookToken)
        {
            this.logger.LogWarning("Wahoo webhook received with invalid token. Rejecting.");
            return this.Unauthorized();
        }

        if (payload.EventType != "workout_summary")
        {
            this.logger.LogInformation(
                "Ignoring Wahoo webhook event type: {EventType}.",
                payload.EventType
            );
            return this.Ok();
        }

        await this.webhookService.ProcessAsync(payload, cancellationToken);
        return this.Ok();
    }
}
