namespace FitSync.Shared.Features.Heartbeat;

using System.Threading;
using System.Threading.Tasks;
using FitSync.Shared.Features.GlobalVariables.DTOs;
using FitSync.Shared.Features.Heartbeat.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class HeartbeatWorker(
    GlobalVariables globalVariables,
    ILogger<HeartbeatWorker> logger,
    IServiceProvider serviceProvider
) : BackgroundService
{
    private readonly GlobalVariables globalVariables = globalVariables;
    private readonly ILogger<HeartbeatWorker> logger = logger;
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly string name = "HearbeatWorker-" + globalVariables.Instance;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("{WorkernName} is starting...", name);
        int heartbeatIntervalMinutes = globalVariables.HeartbeatIntervalMinutes;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                IHeartbeatService heartbeatService =
                    scope.ServiceProvider.GetRequiredService<IHeartbeatService>();
                this.logger.LogTrace("");
                await heartbeatService.UpsertHeartbeatAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                this.logger.LogInformation(
                    "Cancellation requested. {WorkerName} will terminate.",
                    name
                );
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Exception occured in {WorkerName}", name);
            }
            await Task.Delay(TimeSpan.FromMinutes(heartbeatIntervalMinutes), stoppingToken);
        }
        this.logger.LogInformation("{WorkerName} Terminating.", name);
    }
}
