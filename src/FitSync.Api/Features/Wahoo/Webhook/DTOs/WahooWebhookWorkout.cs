namespace FitSync.Api.Features.Wahoo.Webhook.DTOs;

using System.Text.Json.Serialization;

public record WahooWebhookWorkout(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("starts")] DateTime Starts
);
