namespace FitSync.Shared.Features.GlobalVariables;

using FitSync.Database.Enums;
using Microsoft.Extensions.DependencyInjection;

using FitSync.Shared.Features.GlobalVariables.DTOs;

public static class GlobalVariablesFeature
{
    public static IServiceCollection AddGlobalVariables(
        this IServiceCollection services,
        string instanceId,
        string hostname,
        int heartbeatIntervalMinutes,
        ServiceType serviceType
    )
    {
        return services.AddSingleton(
            new GlobalVariables(instanceId, hostname, heartbeatIntervalMinutes, serviceType)
        );
    }
}
