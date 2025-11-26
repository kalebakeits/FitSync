namespace FitSync.ZwiftFetcher.Configuration;

using FitSync.Shared.Configuration;

public class ZwiftFetcherOptions : FetcherOptions
{
    // Zwift API Endpoints
    public required string AuthUrl { get; set; }
    public required string BaseUrl { get; set; }

    // OAuth Client Details (matching the Python implementation)
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}
