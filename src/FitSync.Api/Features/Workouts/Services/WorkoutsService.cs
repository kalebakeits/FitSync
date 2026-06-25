namespace FitSync.Api.Features.Workouts.Services;

using System.Text.Json;
using System.Text.Json.Nodes;
using FitSync.Api.Exceptions;
using FitSync.Api.Features.Workouts.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.SchemaResolver;
using FitSync.Shared.Features.WorkoutBuilder.Services.Writer;
using Microsoft.EntityFrameworkCore;

public class WorkoutsService(
    FitSyncDbContext context,
    IWorkoutWriter workoutWriter,
    IWorkoutSchemaResolver schemaResolver,
    ILogger<WorkoutsService> logger
) : IWorkoutsService
{
    private readonly FitSyncDbContext context = context;
    private readonly IWorkoutWriter workoutWriter = workoutWriter;
    private readonly IWorkoutSchemaResolver schemaResolver = schemaResolver;
    private readonly ILogger<WorkoutsService> logger = logger;

    public async Task<PaginatedWorkoutsResponse> GetWorkoutsAsync(
        Guid userId,
        string? search,
        List<string>? tags,
        int limit,
        int offset
    )
    {
        this.logger.LogInformation("Getting workouts for user: {UserId}", userId);

        IQueryable<Workout> query = this.context.Workouts.Where(w => w.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w => w.Name.ToLower().Contains(search.ToLower()));

        if (tags is { Count: > 0 })
            query = query.Where(w => w.Tags.Any(t => tags.Contains(t)));

        int total = await query.CountAsync();

        List<Workout> workouts = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return new PaginatedWorkoutsResponse(
            workouts.Select(MapToResponse).ToList(),
            total,
            limit,
            offset
        );
    }

    public async Task<WorkoutResponse> GetWorkoutByIdAsync(Guid userId, Guid workoutId)
    {
        this.logger.LogInformation(
            "Getting workout {WorkoutId} for user: {UserId}",
            workoutId,
            userId
        );

        Workout workout = await this.FindWorkoutAsync(userId, workoutId);
        return MapToResponse(workout);
    }

    public async Task<WorkoutResponse> CreateWorkoutAsync(Guid userId, WorkoutSchema schema)
    {
        this.logger.LogInformation("Creating workout for user: {UserId}", userId);

        string schemaJson = JsonSerializer.Serialize(schema);

        (string name, int sport, string? description) = schema.Match<(string, int, string?)>(
            @default => (@default.Name, Convert.ToInt32(@default.Sport), @default.Description),
            poolSwim => (poolSwim.Name, Convert.ToInt32(poolSwim.Sport), poolSwim.Description)
        );

        Workout workout =
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Description = description,
                Tags = [],
                Sport = sport,
                Schema = schemaJson,
            };

        this.context.Workouts.Add(workout);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Created workout {WorkoutId} for user: {UserId}",
            workout.Id,
            userId
        );
        return MapToResponse(workout);
    }

    public async Task<WorkoutResponse> UpdateWorkoutAsync(
        Guid userId,
        Guid workoutId,
        UpdateWorkoutRequest request
    )
    {
        this.logger.LogInformation(
            "Updating workout {WorkoutId} for user: {UserId}",
            workoutId,
            userId
        );

        Workout workout = await this.FindWorkoutAsync(userId, workoutId);
        workout.Name = request.Name;
        workout.Description = request.Description;
        workout.Tags = request.Tags;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Updated workout {WorkoutId} for user: {UserId}",
            workoutId,
            userId
        );
        return MapToResponse(workout);
    }

    public async Task DeleteWorkoutAsync(Guid userId, Guid workoutId)
    {
        this.logger.LogInformation(
            "Deleting workout {WorkoutId} for user: {UserId}",
            workoutId,
            userId
        );

        await this.context.Workouts.Where(w => w.Id == workoutId && w.UserId == userId)
            .ExecuteDeleteAsync();

        this.logger.LogInformation(
            "Deleted workout {WorkoutId} for user: {UserId}",
            workoutId,
            userId
        );
    }

    public async Task<byte[]> DownloadWorkoutAsync(Guid userId, Guid workoutId)
    {
        this.logger.LogInformation(
            "Downloading workout {WorkoutId} for user: {UserId}",
            workoutId,
            userId
        );

        Workout workout = await this.FindWorkoutAsync(userId, workoutId);

        WorkoutSchema? schema = JsonSerializer.Deserialize<WorkoutSchema>(workout.Schema);

        if (schema is null)
            throw new NotFoundException("Workout schema is invalid.");

        TrainingProfile? trainingProfile = await this.context.TrainingProfiles.FirstOrDefaultAsync(
            p => p.UserId == userId
        );

        WorkoutSchema resolvedSchema = trainingProfile is null
            ? schema
            : this.schemaResolver.Resolve(
                schema,
                new ZoneProfile(
                    trainingProfile.FtpWatts,
                    trainingProfile.CyclingMaxHr,
                    trainingProfile.RunningMaxHr,
                    trainingProfile.SwimThresholdHr,
                    trainingProfile.RunningThresholdPaceSeconds,
                    trainingProfile.SwimCssSeconds
                )
            );

        return this.workoutWriter.BuildWorkout(resolvedSchema);
    }

    private async Task<Workout> FindWorkoutAsync(Guid userId, Guid workoutId)
    {
        Workout? workout = await this.context.Workouts.FirstOrDefaultAsync(
            w => w.Id == workoutId && w.UserId == userId
        );

        if (workout is null)
        {
            this.logger.LogWarning(
                "Workout not found - user: {UserId}, workout: {WorkoutId}",
                userId,
                workoutId
            );
            throw new NotFoundException("Workout not found.");
        }

        return workout;
    }

    private static WorkoutResponse MapToResponse(Workout w) =>
        new(
            w.Id,
            w.Name,
            w.Description,
            w.Tags,
            w.Sport,
            w.Schema is not null ? JsonNode.Parse(w.Schema) : null,
            w.CreatedAt,
            w.UpdatedAt
        );
}
