namespace FitSync.Shared.Features.Heartbeat.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.GlobalVariables.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class RoleHeartbeatWriter(
    GlobalVariables globalVariables,
    FitSyncDbContext fitSyncDbContext,
    ILogger<RoleHeartbeatWriter> logger
) : IRoleHeartbeatWriter
{
    private readonly FitSyncDbContext fitSyncDbContext = fitSyncDbContext;
    private readonly ILogger<RoleHeartbeatWriter> logger = logger;
    private readonly string hostname = globalVariables.HostName;

    public async Task WriteAsync(HeartbeatRole role, CancellationToken cancellationToken)
    {
        this.logger.LogDebug(
            "Attempting heartbeat for {Instance} as {ServiceType}",
            role.InstanceId,
            role.ServiceType
        );

        ServiceHeartbeat? heartbeat = await this.fitSyncDbContext.ServiceHeartbeats.FirstOrDefaultAsync(
            h => h.InstanceId == role.InstanceId && h.ServiceType == role.ServiceType,
            cancellationToken
        );

        if (heartbeat == null)
        {
            this.logger.LogDebug(
                "Heartbeat entry for {Instance} as {ServiceType} not found. Creating new entry.",
                role.InstanceId,
                role.ServiceType
            );
            heartbeat = new()
            {
                InstanceId = role.InstanceId,
                Hostname = this.hostname,
                ServiceType = role.ServiceType
            };
            this.fitSyncDbContext.Add(heartbeat);
        }

        DateTime now = DateTime.UtcNow;
        heartbeat.LastHeartbeatAt = now;
        heartbeat.UpdatedAt = now;

        await this.fitSyncDbContext.SaveChangesAsync(cancellationToken);
        this.logger.LogDebug(
            "Updated heartbeat for {Instance} as {ServiceType}",
            role.InstanceId,
            role.ServiceType
        );
    }
}
