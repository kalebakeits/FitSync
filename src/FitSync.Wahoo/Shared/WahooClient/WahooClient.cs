namespace FitSync.Wahoo.Shared.WahooClient;

using System.Text.Json;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.WahooWorkoutBuilder;
using FitSync.Wahoo.Shared.WahooClient.DTOs;
using FitSync.Wahoo.Shared.WahooClient.Services;
using Microsoft.Extensions.Logging;

public class WahooClient(
    IWahooApiService apiService,
    IWahooActivityProcessor activityProcessor,
    IWahooWorkoutBuilder workoutBuilder,
    IWahooWorkoutDurationCalculator durationCalculator,
    ILogger<WahooClient> logger
) : IWahooClient
{
    private readonly IWahooApiService apiService = apiService;
    private readonly IWahooActivityProcessor activityProcessor = activityProcessor;
    private readonly IWahooWorkoutBuilder workoutBuilder = workoutBuilder;
    private readonly IWahooWorkoutDurationCalculator durationCalculator = durationCalculator;
    private readonly ILogger<WahooClient> logger = logger;

    public string ServiceType => "Wahoo";

    public async Task<List<FetchedActivity>> GetActivitiesAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    )
    {
        List<FetchedActivity> activities = await this.activityProcessor.ProcessActivitiesAsync(
            await this.apiService.FetchWorkoutsAsync(integration, lookbackDays, cancellationToken),
            cancellationToken
        );

        this.logger.LogInformation(
            "WahooClient processed {Count} activities for user {UserId}.",
            activities.Count,
            integration.UserId
        );

        return activities;
    }

    public Task<byte[]> DownloadFitFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default
    )
    {
        return this.activityProcessor.DownloadFitFileAsync(fileUrl, cancellationToken);
    }

    public async Task<string> PublishAsync(
        Integration integration,
        WorkoutSchema schema,
        string externalId,
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "WahooClient publishing workout {ExternalId} for user {UserId}.",
            externalId,
            integration.UserId
        );

        WahooPlanDto plan = this.workoutBuilder.Build(schema);

        (string name, int durationMinutes) = schema.Match<(string, int)>(
            @default => (@default.Name, this.durationCalculator.CalculateMinutes(@default.Items)),
            poolSwim => (poolSwim.Name, this.durationCalculator.CalculateMinutes(poolSwim.Items))
        );

        long planId = await this.apiService.PublishPlanAsync(
            integration,
            plan,
            externalId,
            cancellationToken
        );
        long workoutId = await this.apiService.ScheduleWorkoutAsync(
            integration,
            planId,
            name,
            scheduledDate,
            durationMinutes,
            cancellationToken
        );

        return JsonSerializer.Serialize(new { planId, workoutId });
    }

    public async Task RescheduleAsync(
        Integration integration,
        string serviceMetadata,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "WahooClient rescheduling for user {UserId} to {Date}.",
            integration.UserId,
            newDate
        );

        System.Text.Json.JsonElement meta =
            System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(
                serviceMetadata
            );
        long workoutId = meta.GetProperty("workoutId").GetInt64();

        await this.apiService.RescheduleWorkoutAsync(
            integration,
            workoutId,
            newDate,
            cancellationToken
        );

        this.logger.LogInformation(
            "WahooClient rescheduled workoutId={WorkoutId} to {Date} for user {UserId}.",
            workoutId,
            newDate,
            integration.UserId
        );
    }
}
