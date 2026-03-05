namespace FitSync.Wahoo.Fetcher.Configuration;

using FitSync.Shared.Configuration;
using FitSync.Wahoo.Shared.Configuration;

public class WahooFetcherOptions : FetcherOptions
{
    public required WahooClientOptions Client { get; set; }
}
