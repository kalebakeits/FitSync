namespace FitSync.Garmin.Features.ActivityProcessing.Services;

public interface IActivityConsumer
{
    Task ConsumeActivitiesAsync(CancellationToken cancellationToken);
}
