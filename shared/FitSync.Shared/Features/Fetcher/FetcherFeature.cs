namespace FitSync.Shared.Features.Fetcher;

using FitSync.Shared.Configuration;
using FitSync.Shared.Features.ActivityIngest;
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
        IConfigurationSection configSection = getConfigSection();

        // Options are bound (and validated) only for an enabled fetcher: a half that is switched
        // off should not need its own configuration to be present, let alone be able to fail
        // startup over it.
        bool fetcherEnabled = configSection.GetValue("Enabled", true);
        if (!fetcherEnabled)
            return services;

        services.AddActivityIngestFeature();

        services
            .AddOptions<FetcherOptions>()
            .Bind(configSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddScoped<IUserQueuerService, UserQueuerService>()
            .AddScoped<IFetcherService, FetcherService>()
            .AddScoped<IFetcherClient, TFetcherClient>()
            .AddScoped<IActivityPublisher, ActivityPublisher>()
            .AddScoped<IActivityPersistenceService, ActivityPersistenceService>()
            .AddScoped<IFetchOrchestrator, FetchOrchestrator>()
            .AddHostedService<FetcherWorker>();
    }
}
