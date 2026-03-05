namespace FitSync.Shared.Features.Fetcher;

using FitSync.Shared.Configuration;
using FitSync.Shared.Features.Fetcher.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class FetcherFeature
{
    public static IServiceCollection AddFetcher<TQueueService, TFetcherService>(
        this IServiceCollection services,
        Func<IConfigurationSection> getConfigSection
    )
        where TQueueService : class, IUserQueuerService
        where TFetcherService : class, IFetcherService
    {
        services
            .AddOptions<FetcherOptions>()
            .Bind(getConfigSection())
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddScoped<IUserQueuerService, TQueueService>()
            .AddScoped<IFetcherService, TFetcherService>()
            .AddScoped<IDestinationGate, DestinationGate>()
            .AddScoped<IActivityPublisher, ActivityPublisher>()
            .AddScoped<IActivityPersistenceService, ActivityPersistenceService>()
            .AddScoped<IBackpressureMonitor, BackpressureMonitor>()
            .AddScoped<IFetchOrchestrator, FetchOrchestrator>()
            .AddHostedService<FetcherWorker>();
    }
}
