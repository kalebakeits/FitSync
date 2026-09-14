namespace FitSync.Shared.Features.Heartbeat;

using FitSync.Shared.Features.Heartbeat.Services;
using Microsoft.Extensions.DependencyInjection;

public static class HeartbeatFeature
{
    public static IServiceCollection AddHeartbeat(this IServiceCollection services)
    {
        services.AddScoped<IHeartbeatService, HeartbeatService>();
        services.AddScoped<IRoleHeartbeatWriter, RoleHeartbeatWriter>();
        services.AddHostedService<HeartbeatWorker>();

        return services;
    }
}
