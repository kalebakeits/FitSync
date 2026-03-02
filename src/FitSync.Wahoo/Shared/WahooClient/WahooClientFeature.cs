namespace FitSync.Wahoo.Shared.WahooClient;

using FitSync.Wahoo.Shared.Configuration;
using FitSync.Wahoo.Shared.WahooClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class WahooClientFeature
{
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
        services.AddHttpClient<IWahooApiService, WahooApiService>();
        services.AddHttpClient<IWahooActivityProcessor, WahooActivityProcessor>();
        services.AddScoped<IWahooClient, WahooClient>();

        return services;
    }
}
