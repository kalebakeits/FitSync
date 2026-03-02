namespace FitSync.Wahoo.Shared.WahooClient.DTOs;

using System.Text.Json.Serialization;

public sealed class WahooWorkoutDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("starts")]
    public DateTime Starts { get; set; }

    [JsonPropertyName("workout_type_id")]
    public int WorkoutTypeId { get; set; }

    [JsonPropertyName("workout_summary")]
    public WahooWorkoutSummaryDto? WorkoutSummary { get; set; }
}
