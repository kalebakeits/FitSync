namespace FitSync.Garmin.Uploader.Features.OrphanedWork.Services;

public class OrphanedWorkReclaimer(
    IServiceProvider serviceProvider,
    ILogger<OrphanedWorkReclaimer> logger
) : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ILogger<OrphanedWorkReclaimer> logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Orphaned work reclaimer starting...");

        TimeSpan interval = TimeSpan.FromMinutes(2);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = this.serviceProvider.CreateScope();
                IOrphanedActivityReclaimer reclaimer =
                    scope.ServiceProvider.GetRequiredService<IOrphanedActivityReclaimer>();

                await reclaimer.ReclaimOrphanedActivitiesAsync(stoppingToken);
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error reclaiming orphaned work");
                await Task.Delay(interval, stoppingToken);
            }
        }

        this.logger.LogInformation("Orphaned work reclaimer stopped");
    }
}
