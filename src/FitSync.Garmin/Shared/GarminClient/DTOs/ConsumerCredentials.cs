namespace FitSync.Garmin.Shared.GarminClient.DTOs;

using System.Text.Json.Serialization;

public record ConsumerCredentials(
    [property: JsonPropertyName("consumer_key")] string ConsumerKey,
    [property: JsonPropertyName("consumer_secret")] string ConsumerSecret
);
