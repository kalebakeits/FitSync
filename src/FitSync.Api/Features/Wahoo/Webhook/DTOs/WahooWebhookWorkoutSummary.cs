namespace FitSync.Api.Features.Wahoo.Webhook.DTOs;

using System.Text.Json.Serialization;

public sealed class WahooWebhookWorkoutSummary
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("file")]
    public WahooWebhookFile? File { get; set; }

    [JsonPropertyName("workout")]
    public WahooWebhookWorkout? Workout { get; set; }
}
