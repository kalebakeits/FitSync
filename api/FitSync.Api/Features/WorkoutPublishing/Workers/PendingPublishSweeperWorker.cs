namespace FitSync.Api.Features.WorkoutPublishing.Workers;

using FitSync.Api.Configurations;
using FitSync.Api.Features.WorkoutPublishing.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

public class PendingPublishSweeperWorker(
    IServiceProvider serviceProvider,
    IOptions<WorkoutPublishingOptions> options,
    ILogger<PendingPublishSweeperWorker> logger
) : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly IOptions<WorkoutPublishingOptions> options = options;
    private readonly ILogger<PendingPublishSweeperWorker> logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan interval = TimeSpan.FromMinutes(this.options.Value.SweepIntervalMinutes);

        this.logger.LogInformation(
            "Deferred publish sweeper online. Sweeping every {SweepIntervalMinutes} minutes, moves are deferred by {PublishDelayMinutes} minutes.",
            this.options.Value.SweepIntervalMinutes,
            this.options.Value.PublishDelayMinutes
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = this.serviceProvider.CreateScope();
                IPendingPublishSweeper sweeper =
                    scope.ServiceProvider.GetRequiredService<IPendingPublishSweeper>();

                await sweeper.SweepAsync(stoppingToken);
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Deferred publish sweep failed before it could finish. Pending rows keep their PendingPublishAt and the next sweep retries them."
                );
                await Task.Delay(interval, stoppingToken);
            }
        }

        this.logger.LogInformation("Deferred publish sweeper stopping.");
    }
}
