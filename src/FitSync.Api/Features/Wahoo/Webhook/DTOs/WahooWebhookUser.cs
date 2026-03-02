namespace FitSync.Api.Features.Wahoo.Webhook.DTOs;

using System.Text.Json.Serialization;

public sealed class WahooWebhookUser
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}
