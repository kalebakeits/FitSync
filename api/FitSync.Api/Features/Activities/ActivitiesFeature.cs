namespace FitSync.Api.Features.Activities;

using FitSync.Api.Features.Activities.Services;
using FitSync.Shared.Features.ActivityIngest;

public static class ActivitiesFeature
{
    public static IServiceCollection AddActivitiesFeature(this IServiceCollection services)
    {
        services.AddActivityIngestFeature();
        services.AddScoped<IActivitiesService, ActivitiesService>();
        services.AddScoped<IActivityRetryService, ActivityRetryService>();
        return services;
    }
}
