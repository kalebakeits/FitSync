namespace FitSync.Api.Features.WorkoutPublishing.Services;

using FitSync.Api.Configurations;
using FitSync.Api.Features.WorkoutPublishing.DTOs;
using FitSync.Database;
using FitSync.Shared.Features.WorkoutPublisher.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class PendingPublishSweeper(
    FitSyncDbContext dbContext,
    IPendingPublishClaimService claimService,
    IScheduledWorkoutPushService pushService,
    IOptions<WorkoutPublishingOptions> options,
    ILogger<PendingPublishSweeper> logger
) : IPendingPublishSweeper
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IPendingPublishClaimService claimService = claimService;
    private readonly IScheduledWorkoutPushService pushService = pushService;
    private readonly IOptions<WorkoutPublishingOptions> options = options;
    private readonly ILogger<PendingPublishSweeper> logger = logger;

    public async Task<int> SweepAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        DateTime staleClaimCutoff =
            now - TimeSpan.FromMinutes(this.options.Value.PublishDelayMinutes);

        this.logger.LogInformation(
            "Deferred publish sweep starting. Claim cutoff: {StaleClaimCutoff}.",
            staleClaimCutoff
        );

        List<PendingPublishCandidate> due = await this.dbContext
            .ScheduledWorkouts.AsNoTracking()
            .Where(s =>
                s.PendingPublishAt != null
                && s.PendingPublishAt <= now
                && (s.PublishClaimedAt == null || s.PublishClaimedAt < staleClaimCutoff)
            )
            .OrderBy(s => s.PendingPublishAt)
            .Select(s => new PendingPublishCandidate(s.Id, s.UserId, s.ScheduledDate))
            .ToListAsync(cancellationToken);

        this.logger.LogInformation(
            "Deferred publish sweep found {DueCount} due scheduled workouts.",
            due.Count
        );

        int settledCount = 0;

        foreach (PendingPublishCandidate candidate in due)
        {
            bool claimed = await this.claimService.TryClaimAsync(
                candidate.ScheduledWorkoutId,
                now,
                staleClaimCutoff,
                cancellationToken
            );

            if (!claimed)
            {
                this.logger.LogInformation(
                    "Skipping scheduled workout {ScheduledWorkoutId}: another sweep holds the claim.",
                    candidate.ScheduledWorkoutId
                );
                continue;
            }

            try
            {
                bool settled = await this.pushService.PushPendingAsync(
                    candidate.UserId,
                    candidate.ScheduledWorkoutId,
                    this.options.Value.MaxPublishAttempts,
                    cancellationToken
                );

                if (!settled)
                {
                    await this.claimService.ReleaseAsync(
                        candidate.ScheduledWorkoutId,
                        cancellationToken
                    );
                    continue;
                }

                bool completed = await this.claimService.CompleteAsync(
                    candidate.ScheduledWorkoutId,
                    candidate.ScheduledDate,
                    cancellationToken
                );

                if (!completed)
                {
                    await this.claimService.ReleaseAsync(
                        candidate.ScheduledWorkoutId,
                        cancellationToken
                    );
                    continue;
                }

                settledCount++;
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Deferred publish of scheduled workout {ScheduledWorkoutId} failed before settling. PendingPublishAt is kept so the next sweep retries.",
                    candidate.ScheduledWorkoutId
                );
                await this.claimService.ReleaseAsync(
                    candidate.ScheduledWorkoutId,
                    cancellationToken
                );
            }
        }

        this.logger.LogInformation(
            "Deferred publish sweep finished. {SettledCount} of {DueCount} scheduled workouts settled.",
            settledCount,
            due.Count
        );

        return settledCount;
    }
}
