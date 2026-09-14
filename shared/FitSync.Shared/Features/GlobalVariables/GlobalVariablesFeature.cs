namespace FitSync.Shared.Features.GlobalVariables;

using FitSync.Shared.Features.GlobalVariables.DTOs;
using Microsoft.Extensions.DependencyInjection;

public static class GlobalVariablesFeature
{
    public static IServiceCollection AddGlobalVariables(
        this IServiceCollection services,
        IReadOnlyList<HeartbeatRole> roles,
        string instanceId,
        string hostname,
        int heartbeatIntervalMinutes,
        string serviceName
    )
    {
        return services.AddSingleton(
            new GlobalVariables(roles, instanceId, hostname, heartbeatIntervalMinutes, serviceName)
        );
    }
}
