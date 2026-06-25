namespace FitSync.Api.Features.Wahoo.Webhook.DTOs;

using System.Text.Json.Serialization;

public record WahooWebhookPayload(
    [property: JsonPropertyName("event_type")] string EventType,
    [property: JsonPropertyName("webhook_token")] string WebhookToken,
    [property: JsonPropertyName("user")] WahooWebhookUser? User,
    [property: JsonPropertyName("workout_summary")] WahooWebhookWorkoutSummary? WorkoutSummary
);
