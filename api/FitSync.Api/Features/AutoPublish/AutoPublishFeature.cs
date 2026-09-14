namespace FitSync.Api.Features.AutoPublish;

using FitSync.Api.Features.AutoPublish.Services;

public static class AutoPublishFeature
{
    public static IServiceCollection AddAutoPublishFeature(this IServiceCollection services)
    {
        services.AddScoped<IAutoPublishSettingsService, AutoPublishSettingsService>();
        return services;
    }
}
