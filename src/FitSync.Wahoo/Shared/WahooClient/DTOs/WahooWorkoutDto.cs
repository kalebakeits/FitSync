namespace FitSync.Wahoo.Shared.WahooClient.DTOs;

using System.Text.Json.Serialization;

public record WahooWorkoutDto(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("starts")] DateTime Starts,
    [property: JsonPropertyName("workout_type_id")] int WorkoutTypeId,
    [property: JsonPropertyName("workout_summary")] WahooWorkoutSummaryDto? WorkoutSummary
);
