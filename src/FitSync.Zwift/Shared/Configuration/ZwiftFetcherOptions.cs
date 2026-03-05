namespace FitSync.Zwift.Shared.Configuration;

using FitSync.Shared.Configuration;
using FitSync.Shared.Features.RateLimiting;

public class ZwiftFetcherOptions : FetcherOptions
{
    public required string AuthUrl { get; set; }
    public required string BaseUrl { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required List<RateLimit> ZwiftApiRateLimits { get; set; }
    public required List<RateLimit> AmazonS3RateLimits { get; set; }
}
