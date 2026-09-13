namespace FitSync.Api.Features.Wahoo.Webhook.Services;

using FitSync.Api.Features.Wahoo.Webhook.DTOs;

public interface IWahooWebhookService
{
    Task ProcessAsync(WahooWebhookPayload payload, CancellationToken cancellationToken = default);
}
