namespace FitSync.Wahoo.Shared.WahooClient;

using FitSync.Wahoo.Shared.Configuration;
using FitSync.Wahoo.Shared.WahooClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class WahooClientFeature
{
    public static IServiceCollection AddWahooActivityProcessor(this IServiceCollection services)
    {
        services.AddHttpClient<IWahooActivityProcessor, WahooActivityProcessor>();

        return services;
    }

    public static IServiceCollection AddWahooClient(
        this IServiceCollection services,
        Func<IConfigurationSection> getConfigSection
    )
    {
        services
            .AddOptions<WahooClientOptions>()
            .Bind(getConfigSection())
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IWahooAuthService, WahooAuthService>();
        services.AddScoped<IWahooWorkoutDurationCalculator, WahooWorkoutDurationCalculator>();
        services.AddScoped<IWahooRequestFactory, WahooRequestFactory>();
        services.AddHttpClient<IWahooHttpSender, WahooHttpSender>();
        services.AddScoped<IWahooApiService, WahooApiService>();
        services.AddWahooActivityProcessor();
        services.AddScoped<IWahooClient, WahooClient>();

        return services;
    }
}
