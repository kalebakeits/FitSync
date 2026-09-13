namespace FitSync.Api.Features.Activities;

using FitSync.Api.Features.Activities.Services;

public static class ActivitiesFeature
{
    public static IServiceCollection AddActivitiesFeature(this IServiceCollection services)
    {
        services.AddScoped<IActivitiesService, ActivitiesService>();
        services.AddScoped<IActivityRetryService, ActivityRetryService>();
        return services;
    }
}
