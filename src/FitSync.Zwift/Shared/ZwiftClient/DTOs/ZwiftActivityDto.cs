namespace FitSync.Zwift.Shared.ZwiftClient.DTOs;

using System.Text.Json.Serialization;

public record ZwiftActivityProfileDto([property: JsonPropertyName("riding")] bool Riding);

public record ZwiftActivityDto(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("startDate")] string StartDate,
    [property: JsonPropertyName("fitFileBucket")] string FitFileBucket,
    [property: JsonPropertyName("fitFileKey")] string FitFileKey,
    [property: JsonPropertyName("profile")] ZwiftActivityProfileDto? Profile
)
{
    public DateTime GetStartDateTime() =>
        DateTime.SpecifyKind(
            DateTime.Parse(this.StartDate, System.Globalization.CultureInfo.InvariantCulture),
            DateTimeKind.Utc
        );
};
