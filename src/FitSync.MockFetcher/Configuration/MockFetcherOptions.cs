namespace FitSync.MockFetcher.Configuration;

using FitSync.Shared.Configuration;

public class MockFetcherOptions : FetcherOptions
{
    public required bool RunFetcher { get; set; }
    public string? GarminConnectEmail { get; set; }
    public string? GarminConnectPassword { get; set; }
}
