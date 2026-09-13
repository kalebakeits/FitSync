namespace FitSync.Shared.Features.Heartbeat.Services;

using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Shared.Features.GlobalVariables.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class HeartbeatService(
    GlobalVariables globalVariables,
    FitSyncDbContext fitSyncDbContext,
    ILogger<HeartbeatService> logger
) : IHeartbeatService
{
    private readonly FitSyncDbContext fitSyncDbContext = fitSyncDbContext;
    private readonly ILogger<HeartbeatService> logger = logger;
    private readonly string hostname = globalVariables.HostName;
    private readonly string instanceId = globalVariables.Instance;
    private readonly ServiceType serviceType = globalVariables.ServiceType;

    public async Task UpsertHeartbeatAsync(CancellationToken cancellationToken)
    {
        this.logger.LogDebug("Attempting heartbeat for {Instance}", instanceId);
        ServiceHeartbeat? heartbeat = await fitSyncDbContext.ServiceHeartbeats.FirstOrDefaultAsync(
            h => h.InstanceId == this.instanceId,
            cancellationToken
        );

        if (heartbeat == null)
        {
            this.logger.LogDebug(
                "Hearbeat entry for {Instance} not found. Creating new entry.",
                this.instanceId
            );
            heartbeat = new()
            {
                InstanceId = this.instanceId,
                Hostname = hostname,
                ServiceType = serviceType
            };
            this.fitSyncDbContext.Add(heartbeat);
        }

        heartbeat.LastHeartbeatAt = DateTime.UtcNow;
        heartbeat.UpdatedAt = DateTime.UtcNow;

        await fitSyncDbContext.SaveChangesAsync(cancellationToken);
        this.logger.LogDebug("Updated heartbeat");
    }
}
