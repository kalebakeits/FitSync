namespace FitSync.Garmin.Uploader.Features.ActivityProcessing;

using FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ActivityConsumerWorker(
    IServiceProvider serviceProvider,
    ILogger<ActivityConsumerWorker> logger
) : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ILogger<ActivityConsumerWorker> logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Activity Consumer Worker starting... let's eat");

        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = this.serviceProvider.CreateScope();
                IActivityConsumer consumer =
                    scope.ServiceProvider.GetRequiredService<IActivityConsumer>();

                await consumer.ConsumeActivitiesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error in activity consumer. Just doesn't have that hunger anymore"
                );
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        this.logger.LogInformation("Activity Consumer Worker stopped. Bye bye");
    }
}
