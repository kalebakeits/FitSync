namespace FitSync.Api.Features.Wahoo.Webhook.DTOs;

using System.Text.Json.Serialization;

public sealed class WahooWebhookPayload
{
    [JsonPropertyName("event_type")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("webhook_token")]
    public string WebhookToken { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public WahooWebhookUser? User { get; set; }

    [JsonPropertyName("workout_summary")]
    public WahooWebhookWorkoutSummary? WorkoutSummary { get; set; }
}
