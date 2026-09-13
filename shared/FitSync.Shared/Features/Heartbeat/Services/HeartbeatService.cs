namespace FitSync.Shared.Features.Heartbeat.Services;

using FitSync.Shared.Features.GlobalVariables.DTOs;
using Microsoft.Extensions.Logging;

public class HeartbeatService(
    GlobalVariables globalVariables,
    IRoleHeartbeatWriter roleHeartbeatWriter,
    ILogger<HeartbeatService> logger
) : IHeartbeatService
{
    private readonly GlobalVariables globalVariables = globalVariables;
    private readonly IRoleHeartbeatWriter roleHeartbeatWriter = roleHeartbeatWriter;
    private readonly ILogger<HeartbeatService> logger = logger;

    public async Task UpsertHeartbeatAsync(CancellationToken cancellationToken)
    {
        this.logger.LogDebug(
            "Attempting heartbeat for {RoleCount} roles",
            this.globalVariables.Roles.Count
        );

        foreach (HeartbeatRole role in this.globalVariables.Roles)
        {
            await this.roleHeartbeatWriter.WriteAsync(role, cancellationToken);
        }

        this.logger.LogDebug("Updated heartbeats");
    }
}
