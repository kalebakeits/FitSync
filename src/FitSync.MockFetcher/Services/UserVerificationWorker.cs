using FitSync.Database;
using Microsoft.EntityFrameworkCore;

namespace FitSync.MockFetcher.Services;

public class UserVerificationWorker(
    IServiceProvider serviceProvider,
    ILogger<UserVerificationWorker> logger
) : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ILogger<UserVerificationWorker> logger = logger;
    private readonly TimeSpan interval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("User Verification Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                FitSyncDbContext dbContext =
                    scope.ServiceProvider.GetRequiredService<FitSyncDbContext>();

                int verifiedCount = await dbContext
                    .Users.Where(u => !u.IsVerified)
                    .ExecuteUpdateAsync(
                        setters =>
                            setters
                                .SetProperty(u => u.IsVerified, true)
                                .SetProperty(u => u.VerificationToken, (string?)null)
                                .SetProperty(u => u.VerificationTokenExpiresAt, (DateTime?)null),
                        stoppingToken
                    );
                this.logger.LogTrace("Found and verified {verifiedCount} users", verifiedCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while verifying users");
            }

            await Task.Delay(this.interval, stoppingToken);
        }

        logger.LogInformation("User Verification Worker stopped");
    }
}
