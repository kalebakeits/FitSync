namespace FitSync.Wahoo.Shared.WahooClient.DTOs;

using System.Text.Json.Serialization;

public record WahooWorkoutSummaryDto(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("ascent_accum")] string? AscentAccum,
    [property: JsonPropertyName("cadence_avg")] string? CadenceAvg,
    [property: JsonPropertyName("calories_accum")] string? CaloriesAccum,
    [property: JsonPropertyName("distance_accum")] string? DistanceAccum,
    [property: JsonPropertyName("duration_total_accum")] string? DurationTotalAccum,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("file")] WahooFileDto? File
);
