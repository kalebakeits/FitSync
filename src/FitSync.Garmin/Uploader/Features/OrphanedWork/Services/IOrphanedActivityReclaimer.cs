namespace FitSync.Garmin.Uploader.Features.OrphanedWork.Services;

public interface IOrphanedActivityReclaimer
{
    Task ReclaimOrphanedActivitiesAsync(CancellationToken cancellationToken);
}
