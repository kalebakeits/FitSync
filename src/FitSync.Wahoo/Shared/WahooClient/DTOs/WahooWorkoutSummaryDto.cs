namespace FitSync.Wahoo.Shared.WahooClient.DTOs;

using System.Text.Json.Serialization;

public sealed class WahooWorkoutSummaryDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("ascent_accum")]
    public string? AscentAccum { get; set; }

    [JsonPropertyName("cadence_avg")]
    public string? CadenceAvg { get; set; }

    [JsonPropertyName("calories_accum")]
    public string? CaloriesAccum { get; set; }

    [JsonPropertyName("distance_accum")]
    public string? DistanceAccum { get; set; }

    [JsonPropertyName("duration_total_accum")]
    public string? DurationTotalAccum { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("file")]
    public WahooFileDto? File { get; set; }
}
