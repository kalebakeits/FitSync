namespace FitSync.Api.Features.Wahoo.Webhook.DTOs;

using System.Text.Json.Serialization;

public record WahooWebhookUser([property: JsonPropertyName("id")] long Id);
