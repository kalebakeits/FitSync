namespace FitSync.Wahoo.Fetcher.Features.WahooFetcher;

using FitSync.Shared.Features.Fetcher;
using FitSync.Wahoo.Fetcher.Features.WahooFetcher.Services;
using FitSync.Wahoo.Shared.WahooClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class WahooFetcherFeature
{
    public static IServiceCollection AddWahooFetcher(
        this IServiceCollection services,
        Func<IConfigurationSection> getConfigSection
    )
    {
        services
            .AddWahooClient(getConfigSection)
            .AddFetcher<UserQueuerService, FetcherService>(getConfigSection);

        return services;
    }
}
