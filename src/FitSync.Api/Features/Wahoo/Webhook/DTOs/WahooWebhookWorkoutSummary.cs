namespace FitSync.Api.Features.Wahoo.Webhook.DTOs;

using System.Text.Json.Serialization;
using FitSync.Wahoo.Shared.WahooClient.DTOs;

public record WahooWebhookWorkoutSummary(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("file")] WahooFileDto? File,
    [property: JsonPropertyName("workout")] WahooWebhookWorkout? Workout
);
