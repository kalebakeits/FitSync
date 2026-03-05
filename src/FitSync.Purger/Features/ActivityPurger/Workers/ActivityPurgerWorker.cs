namespace FitSync.Purger.Features.ActivityPurger.Workers;

using FitSync.Purger.Configuration;
using FitSync.Purger.Features.ActivityPurger.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ActivityPurgerWorker(
    IServiceProvider serviceProvider,
    IOptions<PurgerOptions> options,
    ILogger<ActivityPurgerWorker> logger
) : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly IOptions<PurgerOptions> options = options;
    private readonly ILogger<ActivityPurgerWorker> logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation(
            "ActivityPurgerWorker started. Interval: {IntervalMinutes} minutes",
            this.options.Value.PurgeIntervalMinutes
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            await this.RunPurgeAsync(stoppingToken);

            TimeSpan delay = TimeSpan.FromMinutes(this.options.Value.PurgeIntervalMinutes);
            this.logger.LogInformation(
                "Next purge in {DelayMinutes} minutes",
                delay.TotalMinutes
            );
            await Task.Delay(delay, stoppingToken);
        }

        this.logger.LogInformation("ActivityPurgerWorker stopping");
    }

    private async Task RunPurgeAsync(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            IActivityPurgerService purgerService =
                scope.ServiceProvider.GetRequiredService<IActivityPurgerService>();

            await purgerService.PurgeAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            this.logger.LogInformation("Purge cycle cancelled during shutdown");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled error during purge cycle. Will retry next interval.");
        }
    }
}
