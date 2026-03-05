namespace FitSync.Purger.Features.ActivityPurger;

using FitSync.Purger.Features.ActivityPurger.Services;
using FitSync.Purger.Features.ActivityPurger.Workers;
using Microsoft.Extensions.DependencyInjection;

public static class ActivityPurgerFeature
{
    public static IServiceCollection AddActivityPurger(this IServiceCollection services)
    {
        services.AddScoped<IActivityPurgerService, ActivityPurgerService>();
        services.AddHostedService<ActivityPurgerWorker>();
        return services;
    }
}
