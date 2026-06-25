namespace FitSync.Api.Features.TrainingProfile.Services;

using FitSync.Api.Features.TrainingProfile.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

public class TrainingProfileService(
    FitSyncDbContext context,
    ILogger<TrainingProfileService> logger
) : ITrainingProfileService
{
    private readonly FitSyncDbContext context = context;
    private readonly ILogger<TrainingProfileService> logger = logger;

    public async Task<TrainingProfileResponse?> GetProfileAsync(Guid userId)
    {
        this.logger.LogInformation("Getting training profile for user: {UserId}", userId);

        TrainingProfile? profile = await this.context.TrainingProfiles.FirstOrDefaultAsync(
            p => p.UserId == userId
        );

        return profile is null ? null : MapToResponse(profile);
    }

    public async Task<TrainingProfileResponse> UpsertProfileAsync(
        Guid userId,
        UpsertTrainingProfileRequest request
    )
    {
        this.logger.LogInformation("Upserting training profile for user: {UserId}", userId);

        TrainingProfile? profile = await this.context.TrainingProfiles.FirstOrDefaultAsync(
            p => p.UserId == userId
        );

        if (profile is null)
        {
            profile = new TrainingProfile { Id = Guid.NewGuid(), UserId = userId };
            this.context.TrainingProfiles.Add(profile);
        }

        profile.FtpWatts = request.FtpWatts;
        profile.CyclingThresholdHr = request.CyclingThresholdHr;
        profile.CyclingMaxHr = request.CyclingMaxHr;
        profile.RunningThresholdHr = request.RunningThresholdHr;
        profile.RunningMaxHr = request.RunningMaxHr;
        profile.RunningThresholdPaceSeconds = request.RunningThresholdPaceSeconds;
        profile.PoolLengthMetres = request.PoolLengthMetres;
        profile.SwimThresholdHr = request.SwimThresholdHr;
        profile.SwimCssSeconds = request.SwimCssSeconds;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation("Upserted training profile for user: {UserId}", userId);
        return MapToResponse(profile);
    }

    private static TrainingProfileResponse MapToResponse(TrainingProfile p) =>
        new(
            p.FtpWatts,
            p.CyclingThresholdHr,
            p.CyclingMaxHr,
            p.RunningThresholdHr,
            p.RunningMaxHr,
            p.RunningThresholdPaceSeconds,
            p.PoolLengthMetres,
            p.SwimThresholdHr,
            p.SwimCssSeconds
        );
}
