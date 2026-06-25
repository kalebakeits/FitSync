namespace FitSync.Wahoo.Shared.WahooClient.Services;

using System.Net.Http.Json;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Shared.Features.RateLimiting;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Wahoo.Shared.Configuration;
using FitSync.Wahoo.Shared.WahooClient.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class WahooApiService(
    IWahooHttpSender sender,
    IWahooRequestFactory requestFactory,
    IOptions<WahooClientOptions> options,
    IRateLimiter rateLimiter,
    ILogger<WahooApiService> logger
) : IWahooApiService
{
    private readonly IWahooHttpSender sender = sender;
    private readonly IWahooRequestFactory requestFactory = requestFactory;
    private readonly IOptions<WahooClientOptions> options = options;
    private readonly IRateLimiter rateLimiter = rateLimiter;
    private readonly ILogger<WahooApiService> logger = logger;

    public async Task<List<WahooWorkoutDto>> FetchWorkoutsAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyList<RateLimit> limits = this.options.Value.RateLimits;
        string baseUrl = $"{this.options.Value.BaseUrl.TrimEnd('/')}/v1/workouts";
        List<WahooWorkoutDto> allWorkouts = [];
        int page = 1;
        DateTime cutoff = DateTime.UtcNow.AddDays(-lookbackDays);

        while (true)
        {
            if (
                limits.Count > 0
                && await this.rateLimiter.RateLimitedReachedAsync(
                    ServiceType.WahooFetcher,
                    limits,
                    cancellationToken
                )
            )
            {
                this.logger.LogWarning(
                    "Wahoo rate limit hit mid-fetch for user {UserId}. Returning partial results.",
                    integration.UserId
                );
                break;
            }

            Dictionary<string, string?> parameters =
                new()
                {
                    ["page"] = page.ToString(),
                    ["per_page"] = "200",
                    ["order"] = "descending",
                    ["sort"] = "starts",
                };

            string pagedUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            HttpResponseMessage response = await this.sender.SendAsync(
                integration,
                () => this.requestFactory.BuildFetchWorkoutsRequest(integration, pagedUrl),
                cancellationToken
            );

            WahooWorkoutsResponse pageResult =
                await response.Content.ReadFromJsonAsync<WahooWorkoutsResponse>(cancellationToken)
                ?? new WahooWorkoutsResponse([], 0, 0, 0);

            List<WahooWorkoutDto> inWindow = pageResult
                .Workouts.Where(w => w.Starts >= cutoff && w.WorkoutSummary?.File?.Url != null)
                .ToList();

            allWorkouts.AddRange(inWindow);

            bool hasMore =
                pageResult.Workouts.Count == 200 && pageResult.Workouts.Last().Starts >= cutoff;
            if (!hasMore)
                break;
            page++;
        }

        this.logger.LogInformation(
            "Fetched {Count} workouts for user {UserId} within last {Days} days.",
            allWorkouts.Count,
            integration.UserId,
            lookbackDays
        );

        return allWorkouts;
    }

    public async Task<long> PublishPlanAsync(
        Integration integration,
        WahooPlanDto plan,
        string externalId,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Publishing plan {ExternalId} to Wahoo for user {UserId}.",
            externalId,
            integration.UserId
        );

        HttpResponseMessage response = await this.sender.SendAsync(
            integration,
            () => this.requestFactory.BuildPublishPlanRequest(integration, plan, externalId),
            cancellationToken
        );

        WahooPlanResponse planResponse =
            await response.Content.ReadFromJsonAsync<WahooPlanResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Wahoo plan response could not be parsed.");

        this.logger.LogInformation(
            "Successfully published plan {ExternalId} (id={PlanId}) for user {UserId}.",
            externalId,
            planResponse.Id,
            integration.UserId
        );

        return planResponse.Id;
    }

    public async Task UpdatePlanAsync(
        Integration integration,
        long planId,
        WahooPlanDto plan,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Updating plan {PlanId} on Wahoo for user {UserId}.",
            planId,
            integration.UserId
        );

        await this.sender.SendAsync(
            integration,
            () => this.requestFactory.BuildUpdatePlanRequest(integration, planId, plan),
            cancellationToken
        );

        this.logger.LogInformation(
            "Successfully updated plan {PlanId} for user {UserId}.",
            planId,
            integration.UserId
        );
    }

    public async Task<long> ScheduleWorkoutAsync(
        Integration integration,
        long planId,
        string name,
        DateOnly scheduledDate,
        int durationMinutes,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Scheduling workout '{Name}' on {Date} to Wahoo for user {UserId}.",
            name,
            scheduledDate,
            integration.UserId
        );

        HttpResponseMessage response = await this.sender.SendAsync(
            integration,
            () =>
                this.requestFactory.BuildScheduleWorkoutRequest(
                    integration,
                    planId,
                    name,
                    scheduledDate,
                    durationMinutes
                ),
            cancellationToken
        );

        WahooScheduledWorkoutResponse workoutResponse =
            await response.Content.ReadFromJsonAsync<WahooScheduledWorkoutResponse>(
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                "Wahoo scheduled workout response could not be parsed."
            );

        this.logger.LogInformation(
            "Successfully scheduled workout (id={WorkoutId}) on {Date} for user {UserId}.",
            workoutResponse.Id,
            scheduledDate,
            integration.UserId
        );

        return workoutResponse.Id;
    }

    public async Task RescheduleWorkoutAsync(
        Integration integration,
        long workoutId,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Rescheduling Wahoo workout {WorkoutId} to {Date} for user {UserId}.",
            workoutId,
            newDate,
            integration.UserId
        );

        await this.sender.SendAsync(
            integration,
            () =>
                this.requestFactory.BuildRescheduleWorkoutRequest(integration, workoutId, newDate),
            cancellationToken
        );

        this.logger.LogInformation(
            "Rescheduled Wahoo workout {WorkoutId} to {Date} for user {UserId}.",
            workoutId,
            newDate,
            integration.UserId
        );
    }
}
