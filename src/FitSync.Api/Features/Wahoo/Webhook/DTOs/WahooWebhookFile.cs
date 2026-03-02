namespace FitSync.Api.Features.Wahoo.Webhook.DTOs;

using System.Text.Json.Serialization;

public sealed class WahooWebhookFile
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
