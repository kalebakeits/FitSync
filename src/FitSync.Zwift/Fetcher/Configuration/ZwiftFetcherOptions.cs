namespace FitSync.Zwift.Fetcher.Configuration;

using FitSync.Database.Enums;
using FitSync.Shared.Configuration;

public class ZwiftFetcherOptions : FetcherOptions
{
    // Zwift API Endpoints
    public required string AuthUrl { get; set; }
    public required string BaseUrl { get; set; }

    // OAuth Client Details (matching the Python implementation)
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required int AmazonS3RateLimit { get; set; }
    public required int ZwfitApiRateLimit { get; set; }
}
