namespace FitSync.Garmin.Features.OrphanedWork.Services;

public interface IOrphanedActivityReclaimer
{
    Task ReclaimOrphanedActivitiesAsync(CancellationToken cancellationToken);
}
