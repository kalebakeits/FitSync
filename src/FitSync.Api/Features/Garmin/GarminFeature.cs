namespace FitSync.Api.Features.Garmin;

using FitSync.Garmin.Shared.GarminClient;
using Microsoft.Extensions.DependencyInjection;

public static class GarminFeature
{
    public static IServiceCollection AddGarminFeature(this IServiceCollection services)
    {
        services.AddGarminClient();
        return services;
    }
}
