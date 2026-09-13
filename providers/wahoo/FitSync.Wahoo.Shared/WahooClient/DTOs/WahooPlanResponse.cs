namespace FitSync.Wahoo.Shared.WahooClient.DTOs;

using System.Text.Json.Serialization;

public record WahooPlanResponse([property: JsonPropertyName("id")] long Id);

public record WahooScheduledWorkoutResponse([property: JsonPropertyName("id")] long Id);
