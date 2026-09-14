namespace FitSync.Garmin.Shared.Configuration;

using FitSync.Shared.Configuration;
using FitSync.Shared.Features.RateLimiting;

public class GarminFetcherOptions : FetcherOptions
{
    public required string BaseUrl { get; set; }
    public required List<RateLimit> GarminApiRateLimits { get; set; }
}
