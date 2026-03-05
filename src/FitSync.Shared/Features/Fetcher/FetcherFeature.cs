namespace FitSync.Shared.Features.Fetcher;

using FitSync.Shared.Configuration;
using FitSync.Shared.Features.Fetcher.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class FetcherFeature
{
    public static IServiceCollection AddFetcher<TFetcherClient>(
        this IServiceCollection services,
        Func<IConfigurationSection> getConfigSection
    )
        where TFetcherClient : class, IFetcherClient
    {
        services
            .AddOptions<FetcherOptions>()
            .Bind(getConfigSection())
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddScoped<IUserQueuerService, UserQueuerService>()
            .AddScoped<IFetcherService, FetcherService>()
            .AddScoped<IFetcherClient, TFetcherClient>()
            .AddScoped<IDestinationGate, DestinationGate>()
            .AddScoped<IActivityPublisher, ActivityPublisher>()
            .AddScoped<IActivityPersistenceService, ActivityPersistenceService>()
            .AddScoped<IBackpressureMonitor, BackpressureMonitor>()
            .AddScoped<IFetchOrchestrator, FetchOrchestrator>()
            .AddHostedService<FetcherWorker>();
    }
}
