namespace FitSync.Api.Features.Wahoo;

using FitSync.Api.Features.Wahoo.Services;
using FitSync.Api.Features.Wahoo.Webhook.Services;
using FitSync.Shared.Features.Fetcher.Services;
using SharedWahooClient = FitSync.Wahoo.Shared.WahooClient;
using SharedWahooServices = FitSync.Wahoo.Shared.WahooClient.Services;

public static class WahooServiceExtensions
{
    public static IServiceCollection AddWahooFeature(this IServiceCollection services)
    {
        services.AddHttpClient<IWahooOAuthService, WahooAuthService>();
        services.AddScoped<IWahooConnectionService, WahooConnectionService>();

        services.AddHttpClient<SharedWahooServices.IWahooAuthService, SharedWahooServices.WahooAuthService>();
        services.AddHttpClient<SharedWahooServices.IWahooApiService, SharedWahooServices.WahooApiService>();
        services.AddHttpClient<SharedWahooServices.IWahooActivityProcessor, SharedWahooServices.WahooActivityProcessor>();
        services.AddScoped<SharedWahooClient.IWahooClient, SharedWahooClient.WahooClient>();

        services.AddScoped<IWahooWebhookService, WahooWebhookService>();
        services.AddScoped<IActivityPublisher, ActivityPublisher>();

        return services;
    }
}
