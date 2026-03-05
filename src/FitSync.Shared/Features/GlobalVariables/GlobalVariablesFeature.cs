namespace FitSync.Shared.Features.GlobalVariables;

using FitSync.Database.Enums;
using FitSync.Shared.Features.GlobalVariables.DTOs;
using Microsoft.Extensions.DependencyInjection;

public static class GlobalVariablesFeature
{
    public static IServiceCollection AddGlobalVariables(
        this IServiceCollection services,
        string instanceId,
        string hostname,
        int heartbeatIntervalMinutes,
        ServiceType serviceType,
        string serviceName
    )
    {
        return services.AddSingleton(
            new GlobalVariables(
                instanceId,
                hostname,
                heartbeatIntervalMinutes,
                serviceType,
                serviceName
            )
        );
    }
}
