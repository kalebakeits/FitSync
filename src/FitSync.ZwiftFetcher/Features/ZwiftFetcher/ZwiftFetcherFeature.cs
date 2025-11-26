namespace FitSync.ZwiftFetcher.Features.ZwiftFetcher;

using FitSync.Shared.Features.Fetcher;
using FitSync.ZwiftFetcher.Features.ZwiftFetcher.Services;
using Microsoft.Extensions.Configuration;

public static class ZwiftFetcherFeature
{
    public static IServiceCollection AddZwiftFetcher(
        this IServiceCollection services,
        Func<IConfigurationSection> getConfigSection
    )
    {
        return services.AddFetcher<UserQueuerService, FetcherService>(getConfigSection);
    }
}
