namespace FitSync.Garmin.Shared.GarminClient.DTOs;

using System.Globalization;
using System.Text.Json.Serialization;

public record GarminActivityTypeDto(
    [property: JsonPropertyName("typeId")] long TypeId,
    [property: JsonPropertyName("typeKey")] string? TypeKey
);

public record GarminActivityDto(
    [property: JsonPropertyName("activityId")] long ActivityId,
    [property: JsonPropertyName("activityName")] string? ActivityName,
    [property: JsonPropertyName("startTimeGMT")] string? StartTimeGmt,
    [property: JsonPropertyName("duration")] double Duration,
    [property: JsonPropertyName("activityType")] GarminActivityTypeDto? ActivityType
)
{
    // Garmin sends startTimeGMT as "yyyy-MM-dd HH:mm:ss" with no timezone
    // designator, so it has to be read as UTC rather than local time.
    public DateTime GetStartDateTime() =>
        DateTime.ParseExact(
            this.StartTimeGmt ?? string.Empty,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal
        );
}
