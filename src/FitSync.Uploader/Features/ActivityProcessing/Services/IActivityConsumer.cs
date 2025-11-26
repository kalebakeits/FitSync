namespace FitSync.Uploader.Features.ActivityProcessing.Services;

public interface IActivityConsumer
{
    Task ConsumeActivitiesAsync(CancellationToken cancellationToken);
}
