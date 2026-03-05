namespace FitSync.Shared.Features.Fetcher;

using Humanizer;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class FetcherWorker(IServiceProvider serviceProvider, ILogger<FetcherWorker> logger)
    : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ILogger<FetcherWorker> logger = logger;
    private const int sleepTimeMinutes = 1; // All fetchers sleep 1 min but per-user processing uses a configurable linear backoff.

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Fetcher Worker starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = this.serviceProvider.CreateScope();
                IUserQueuerService userQueuerService =
                    scope.ServiceProvider.GetRequiredService<IUserQueuerService>();
                IBackpressureMonitor backpressureMonitor =
                    scope.ServiceProvider.GetRequiredService<IBackpressureMonitor>();
                IFetchOrchestrator fetchOrchestrator =
                    scope.ServiceProvider.GetRequiredService<IFetchOrchestrator>();

                bool shouldFetch = await backpressureMonitor.ShouldFetchAsync(stoppingToken);
                if (!shouldFetch)
                {
                    this.logger.LogWarning(
                        "Backpressure detected - skipping fetch cycle. Waiting {Minutes} minutes...",
                        sleepTimeMinutes
                    );
                    await Task.Delay(TimeSpan.FromMinutes(sleepTimeMinutes), stoppingToken);
                    continue;
                }

                while (true)
                {
                    User[] users = await userQueuerService.GetDueUsersAsync();
                    if (users.Length == 0)
                    {
                        this.logger.LogInformation("No more due users to process");
                        break;
                    }

                    this.logger.LogInformation("Found {Count} users to process", users.Length);
                    await fetchOrchestrator.ProcessUsersAsync(users, stoppingToken);
                    await userQueuerService.ReleaseUsersAsync(users);
                }

                this.logger.LogInformation(
                    "Waiting {Minutes} minutes until next fetch cycle...",
                    sleepTimeMinutes
                );
                await Task.Delay(TimeSpan.FromMinutes(sleepTimeMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred during fetch cycle");
            }
        }

        this.logger.LogInformation("Fetcher Worker stopped");
    }
}
