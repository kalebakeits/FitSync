namespace FitSync.Api.Features.Mcp;

using System.ComponentModel;
using System.Security.Claims;
using FitSync.Api.Features.Credentials.Services;
using FitSync.Api.Features.TrainingProfile.DTOs;
using FitSync.Api.Features.TrainingProfile.Services;
using FitSync.Api.Features.WorkoutPublishing.DTOs;
using FitSync.Api.Features.WorkoutPublishing.Services;
using FitSync.Api.Features.Workouts.DTOs;
using FitSync.Api.Features.Workouts.Services;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using ModelContextProtocol.Server;

[McpServerToolType]
public class FitSyncMcpTools(
    IWorkoutsService workoutsService,
    IWorkoutPublishingService workoutPublishingService,
    ITrainingProfileService trainingProfileService,
    ICredentialsService credentialsService,
    IHttpContextAccessor httpContextAccessor
)
{
    private readonly IWorkoutsService workoutsService = workoutsService;
    private readonly IWorkoutPublishingService workoutPublishingService = workoutPublishingService;
    private readonly ITrainingProfileService trainingProfileService = trainingProfileService;
    private readonly ICredentialsService credentialsService = credentialsService;
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

    private Guid UserId =>
        Guid.Parse(
            this.httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

    [
        McpServerTool,
        Description(
            "List the user's connected device services that support workout publishing (e.g. Garmin, Wahoo). "
                + "Call this before PublishWorkout to get valid serviceType values for this user. "
                + "Returns serviceType (use this string in PublishWorkout) and enabled (false means the integration has repeated failures)."
        )
    ]
    public async Task<List<ConnectedServiceSummary>> GetConnectedServices()
    {
        List<FitSync.Api.Features.Credentials.DTOs.CredentialResponse> credentials =
            await this.credentialsService.GetCredentialsAsync(this.UserId);
        return credentials
            .Select(c => new ConnectedServiceSummary(c.ServiceType, c.Enabled))
            .ToList();
    }

    [
        McpServerTool,
        Description(
            "List workouts in the library. Returns a summary for each workout: id, name, description, sport (2=Cycling, 3=Running, 5=Swimming), subSport, and tags. "
                + "Use GetWorkout to retrieve the full schema for a specific workout."
        )
    ]
    public async Task<List<WorkoutSummary>> ListWorkouts(
        [Description("Optional search term to filter by name")] string? search = null,
        [Description("Optional list of tags to filter by")] List<string>? tags = null
    )
    {
        PaginatedWorkoutsResponse result = await this.workoutsService.GetWorkoutsAsync(
            this.UserId,
            search,
            tags,
            100,
            0
        );
        return result
            .Items.Select(w => new WorkoutSummary(w.Id, w.Name, w.Description, w.Sport, w.Tags))
            .ToList();
    }

    [
        McpServerTool,
        Description(
            "Get the full schema for a single workout by its id. Use this when you need the workout's step-by-step structure."
        )
    ]
    public async Task<WorkoutResponse> GetWorkout([Description("The workout id")] Guid workoutId)
    {
        return await this.workoutsService.GetWorkoutByIdAsync(this.UserId, workoutId);
    }

    [McpServerTool, Description(FitSync.Api.Generated.WorkoutGenerationPrompt.Text)]
    public async Task<WorkoutResponse> CreateWorkout(
        [Description("The full workout schema including optional description")] WorkoutSchema schema
    )
    {
        return await this.workoutsService.CreateWorkoutAsync(this.UserId, schema);
    }

    [
        McpServerTool,
        Description(
            "List scheduled workouts, optionally filtered by date range. "
                + "Returns a summary for each entry: id (use this for MoveScheduledWorkout or DeleteScheduledWorkout), "
                + "workoutId (the library workout), workoutName, sport, and scheduledDate. "
                + "Use GetScheduledWorkout to retrieve full details for a specific entry."
        )
    ]
    public async Task<List<ScheduledWorkoutSummary>> ListScheduledWorkouts(
        [Description("Start date (yyyy-MM-dd), inclusive")] string? from = null,
        [Description("End date (yyyy-MM-dd), inclusive")] string? to = null
    )
    {
        DateOnly? fromDate = from is not null ? DateOnly.Parse(from) : null;
        DateOnly? toDate = to is not null ? DateOnly.Parse(to) : null;
        List<ScheduledWorkoutResponse> results =
            await this.workoutPublishingService.GetScheduledWorkoutsAsync(
                this.UserId,
                fromDate,
                toDate
            );
        return results
            .Select(s => new ScheduledWorkoutSummary(
                s.Id,
                s.WorkoutId,
                s.WorkoutName,
                s.Sport,
                s.ScheduledDate
            ))
            .ToList();
    }

    [
        McpServerTool,
        Description(
            "Get full details for a single scheduled workout entry by its id."
        )
    ]
    public async Task<ScheduledWorkoutResponse> GetScheduledWorkout(
        [Description("The scheduled workout id")] Guid scheduledWorkoutId
    )
    {
        List<ScheduledWorkoutResponse> all =
            await this.workoutPublishingService.GetScheduledWorkoutsAsync(
                this.UserId,
                null,
                null
            );
        ScheduledWorkoutResponse? entry = all.FirstOrDefault(s => s.Id == scheduledWorkoutId);
        if (entry is null)
            throw new KeyNotFoundException($"Scheduled workout {scheduledWorkoutId} not found.");
        return entry;
    }

    [
        McpServerTool,
        Description(
            "Schedule a workout on a date, optionally pushing it to a connected device service. "
                + "Call GetConnectedServices first to see which serviceTypes are available for this user. "
                + "If serviceType is omitted, the workout is added to the calendar only and not sent to any device."
        )
    ]
    public async Task PublishWorkout(
        [Description("The workout library id (from ListWorkouts)")] Guid workoutId,
        [Description("Date to schedule the workout (yyyy-MM-dd)")] string scheduledDate,
        [Description("Optional: destination service (from GetConnectedServices). Omit to schedule calendar-only.")] string? serviceType = null
    )
    {
        await this.workoutPublishingService.PublishAsync(
            this.UserId,
            workoutId,
            serviceType,
            DateOnly.Parse(scheduledDate)
        );
    }

    [McpServerTool, Description("Move a scheduled workout to a different date.")]
    public async Task<ScheduledWorkoutResponse> MoveScheduledWorkout(
        [Description("The scheduled workout id")] Guid scheduledWorkoutId,
        [Description("New date (yyyy-MM-dd)")] string newDate
    )
    {
        return await this.workoutPublishingService.MoveScheduledWorkoutAsync(
            this.UserId,
            scheduledWorkoutId,
            DateOnly.Parse(newDate)
        );
    }

    [
        McpServerTool,
        Description(
            "Remove a scheduled workout from the calendar but not from the library."
        )
    ]
    public async Task DeleteScheduledWorkout(
        [Description("The scheduled workout id")] Guid scheduledWorkoutId
    )
    {
        await this.workoutPublishingService.DeleteScheduledWorkoutAsync(
            this.UserId,
            scheduledWorkoutId
        );
    }

    [
        McpServerTool,
        Description(
            "Get the user's training profile (FTP, cycling/running/swim HR zones, paces, pool length). "
                + "Returns null if not yet configured. "
                + "All fields are nullable; a null field means that metric has not been set."
        )
    ]
    public async Task<TrainingProfileResponse?> GetTrainingProfile()
    {
        return await this.trainingProfileService.GetProfileAsync(this.UserId);
    }

    [
        McpServerTool,
        Description(
            "Create or update the user's training profile."
                + "Only the fields you pass are meaningful; omitted fields are set to null."
                + "Fields: ftpWatts (cycling power threshold), cyclingThresholdHr / cyclingMaxHr (cycling HR), "
                + "runningThresholdHr / runningMaxHr (running HR), runningThresholdPaceSeconds (threshold pace in seconds per km), "
                + "poolLengthMetres, swimThresholdHr, swimCssSeconds (critical swim speed in seconds per 100 m)."
        )
    ]
    public async Task<TrainingProfileResponse> UpdateTrainingProfile(
        [Description("FTP in watts (cycling power threshold)")] int? ftpWatts = null,
        [Description("Cycling threshold heart rate in BPM")] int? cyclingThresholdHr = null,
        [Description("Cycling maximum heart rate in BPM")] int? cyclingMaxHr = null,
        [Description("Running threshold heart rate in BPM")] int? runningThresholdHr = null,
        [Description("Running maximum heart rate in BPM")] int? runningMaxHr = null,
        [Description("Running threshold pace in seconds per km (e.g. 300 = 5:00/km)")]
            int? runningThresholdPaceSeconds = null,
        [Description("Pool length in metres (e.g. 25 or 50)")] float? poolLengthMetres = null,
        [Description("Swim threshold heart rate in BPM")] int? swimThresholdHr = null,
        [Description("Critical swim speed in seconds per 100 m (e.g. 120 = 2:00/100 m)")]
            int? swimCssSeconds = null
    )
    {
        UpsertTrainingProfileRequest request =
            new(
                ftpWatts,
                cyclingThresholdHr,
                cyclingMaxHr,
                runningThresholdHr,
                runningMaxHr,
                runningThresholdPaceSeconds,
                poolLengthMetres,
                swimThresholdHr,
                swimCssSeconds
            );
        return await this.trainingProfileService.UpsertProfileAsync(this.UserId, request);
    }
}

public record WorkoutSummary(Guid Id, string Name, string? Description, int Sport, List<string> Tags);

public record ScheduledWorkoutSummary(
    Guid Id,
    Guid WorkoutId,
    string WorkoutName,
    int Sport,
    DateOnly ScheduledDate
);

public record ConnectedServiceSummary(string ServiceType, bool Enabled);
