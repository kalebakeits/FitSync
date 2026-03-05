namespace FitSync.Api.Features.Wahoo;

using FitSync.Api.Features.Wahoo.Services;
using FitSync.Api.Features.Wahoo.Webhook.Services;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.Wahoo.Shared.WahooClient;
using Microsoft.Extensions.Configuration;

public static class WahooServiceExtensions
{
    public static IServiceCollection AddWahooFeature(
        this IServiceCollection services,
        Func<IConfigurationSection> getConfigSection
    )
    {
        services.AddHttpClient<IWahooOAuthService, WahooAuthService>();
        services.AddScoped<IWahooConnectionService, WahooConnectionService>();
        services.AddWahooClient(getConfigSection);
        services.AddScoped<IWahooWebhookService, WahooWebhookService>();
        services.AddScoped<IActivityPublisher, ActivityPublisher>();

        return services;
    }
}
