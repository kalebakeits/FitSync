namespace FitSync.Shared.Features.Heartbeat.Services;

public interface IHeartbeatService
{
    Task UpsertHeartbeatAsync(CancellationToken cancellationToken);
}
