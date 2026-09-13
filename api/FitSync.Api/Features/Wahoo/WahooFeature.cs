namespace FitSync.Api.Features.Wahoo;

using FitSync.Api.Features.Wahoo.Services;
using FitSync.Api.Features.Wahoo.Webhook.Services;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.Shared.Features.WorkoutPublisher.Services;
using FitSync.Wahoo.Shared.Configuration;
using FitSync.Wahoo.Shared.WahooClient;
using FitSync.Wahoo.Shared.WahooClient.Services;
using Microsoft.Extensions.Configuration;

public static class WahooFeature
{
    public static IServiceCollection AddWahooFeature(
        this IServiceCollection services,
        Func<IConfigurationSection> getConfigSection
    )
    {
        services
            .AddOptions<WahooClientOptions>()
            .Bind(getConfigSection())
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<
            IWahooOAuthService,
            FitSync.Api.Features.Wahoo.Services.WahooAuthService
        >();
        services.AddScoped<IWahooConnectionService, WahooConnectionService>();
        services.AddWahooActivityProcessor();
        services.AddScoped<IWahooWebhookService, WahooWebhookService>();
        services.AddScoped<IActivityPublisher, ActivityPublisher>();
        services.AddHttpClient<
            IWahooAuthService,
            FitSync.Wahoo.Shared.WahooClient.Services.WahooAuthService
        >();
        services.AddScoped<IWahooWorkoutDurationCalculator, WahooWorkoutDurationCalculator>();
        services.AddScoped<IWahooRequestFactory, WahooRequestFactory>();
        services.AddHttpClient<IWahooHttpSender, WahooHttpSender>();
        services.AddScoped<IWahooApiService, WahooApiService>();
        services.AddScoped<IWahooClient, WahooClient>();
        services.AddScoped<IWorkoutPublisherClient, WahooClient>();

        return services;
    }
}
