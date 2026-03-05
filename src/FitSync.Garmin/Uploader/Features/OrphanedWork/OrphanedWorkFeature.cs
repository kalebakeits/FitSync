namespace FitSync.Garmin.Uploader.Features.OrphanedWork;

using FitSync.Garmin.Uploader.Features.OrphanedWork.Services;

public static class OrphanedWorkFeature
{
    public static IServiceCollection AddOrphanedWorkReclaimer(this IServiceCollection services)
    {
        services.AddScoped<IOrphanedActivityReclaimer, OrphanedActivityReclaimer>();
        services.AddHostedService<OrphanedWorkReclaimer>();

        return services;
    }
}
