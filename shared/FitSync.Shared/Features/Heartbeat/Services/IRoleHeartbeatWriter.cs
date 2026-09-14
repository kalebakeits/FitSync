namespace FitSync.Shared.Features.Heartbeat.Services;

using FitSync.Shared.Features.GlobalVariables.DTOs;

public interface IRoleHeartbeatWriter
{
    Task WriteAsync(HeartbeatRole role, CancellationToken cancellationToken);
}
