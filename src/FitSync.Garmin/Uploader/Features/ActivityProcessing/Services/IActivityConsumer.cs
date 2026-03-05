namespace FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;

public interface IActivityConsumer
{
    Task ConsumeActivitiesAsync(CancellationToken cancellationToken);
}
