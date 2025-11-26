namespace FitSync.ZwiftFetcher.Features.ZwiftClient.DTOs;

using Newtonsoft.Json;

public record ZwiftActivityDto(
    [property: JsonProperty("id")] long Id,
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("startDate")] string StartDate,
    [property: JsonProperty("fitFileBucket")] string FitFileBucket,
    [property: JsonProperty("fitFileKey")] string FitFileKey
)
{
    public DateTime GetStartDateTime() =>
        DateTime.SpecifyKind(
            DateTime.Parse(this.StartDate, System.Globalization.CultureInfo.InvariantCulture),
            DateTimeKind.Utc
        );
};
