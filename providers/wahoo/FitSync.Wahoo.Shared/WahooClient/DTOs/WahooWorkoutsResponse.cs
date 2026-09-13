namespace FitSync.Wahoo.Shared.WahooClient.DTOs;

using System.Text.Json.Serialization;

public record WahooWorkoutsResponse(
    [property: JsonPropertyName("workouts")] List<WahooWorkoutDto> Workouts,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("per_page")] int PerPage
);
