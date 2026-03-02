namespace FitSync.Zwift.Fetcher.Features.ZwiftClient.DTOs;

using Newtonsoft.Json;

public record ZwiftActivityProfileDto([property: JsonProperty("riding")] bool Riding);

public record ZwiftActivityDto(
    [property: JsonProperty("id")] long Id,
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("startDate")] string StartDate,
    [property: JsonProperty("fitFileBucket")] string FitFileBucket,
    [property: JsonProperty("fitFileKey")] string FitFileKey,
    [property: JsonProperty("profile")] ZwiftActivityProfileDto? Profile
)
{
    public DateTime GetStartDateTime() =>
        DateTime.SpecifyKind(
            DateTime.Parse(this.StartDate, System.Globalization.CultureInfo.InvariantCulture),
            DateTimeKind.Utc
        );
};
