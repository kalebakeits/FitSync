namespace FitSync.Garmin.Shared.GarminClient;

using System.Text.Json;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.AuthData;
using FitSync.Garmin.Shared.GarminClient.Services;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.GarminWorkoutBuilder;
using FitSync.Shared.Features.WorkoutPublisher.Services;
using Microsoft.Extensions.Logging;

public class GarminClient(
    IGarminApiClient apiClient,
    IGarminAuthService authService,
    IGarminWorkoutBuilder workoutBuilder,
    IEncryptionService encryptionService,
    ILogger<GarminClient> logger
) : IWorkoutPublisherClient
{
    private readonly IGarminApiClient apiClient = apiClient;
    private readonly IGarminAuthService authService = authService;
    private readonly IGarminWorkoutBuilder workoutBuilder = workoutBuilder;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<GarminClient> logger = logger;

    public string ServiceType => "Garmin";

    public async Task<string> PublishAsync(
        Integration integration,
        WorkoutSchema schema,
        string externalId,
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "GarminClient publishing workout {ExternalId} for user {UserId}.",
            externalId,
            integration.UserId
        );

        await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);
        GarminAuthData authData = integration.GetAuthData<GarminAuthData>(this.encryptionService);
        string accessToken = authData.OAuth2AccessToken!;

        GarminWorkoutDto workout = this.workoutBuilder.Build(schema);
        string workoutJson = JsonSerializer.Serialize(workout);

        long workoutId = await this.apiClient.CreateWorkoutAsync(
            workoutJson,
            accessToken,
            cancellationToken
        );
        long workoutScheduleId = await this.apiClient.ScheduleWorkoutAsync(
            workoutId,
            scheduledDate,
            accessToken,
            cancellationToken
        );

        this.logger.LogInformation(
            "GarminClient published workout {WorkoutId} (scheduleId={ScheduleId}) on {Date} for user {UserId}.",
            workoutId,
            workoutScheduleId,
            scheduledDate,
            integration.UserId
        );

        return JsonSerializer.Serialize(new { workoutId, workoutScheduleId });
    }

    public async Task RescheduleAsync(
        Integration integration,
        string serviceMetadata,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "GarminClient rescheduling for user {UserId} to {Date}.",
            integration.UserId,
            newDate
        );

        await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);
        GarminAuthData authData = integration.GetAuthData<GarminAuthData>(this.encryptionService);
        string accessToken = authData.OAuth2AccessToken!;

        System.Text.Json.JsonElement meta =
            JsonSerializer.Deserialize<System.Text.Json.JsonElement>(serviceMetadata);
        long workoutScheduleId = meta.GetProperty("workoutScheduleId").GetInt64();

        await this.apiClient.RescheduleWorkoutAsync(
            workoutScheduleId,
            newDate,
            accessToken,
            cancellationToken
        );

        this.logger.LogInformation(
            "GarminClient rescheduled scheduleId={ScheduleId} to {Date} for user {UserId}.",
            workoutScheduleId,
            newDate,
            integration.UserId
        );
    }
}
