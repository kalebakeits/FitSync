namespace FitSync.Api.Features.Activities.Services;

using FitSync.Api.Exceptions;
using FitSync.Api.Features.Activities.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

public class ActivitiesService(FitSyncDbContext context, ILogger<ActivitiesService> logger)
    : IActivitiesService
{
    private readonly FitSyncDbContext context = context;
    private readonly ILogger<ActivitiesService> logger = logger;

    public async Task<PaginatedActivitiesResponse> GetActivitiesAsync(
        Guid userId,
        int limit,
        int offset,
        DateOnly? from,
        DateOnly? to
    )
    {
        this.logger.LogInformation(
            "Getting activities for user: {UserId}, limit: {Limit}, offset: {Offset}, from: {From}, to: {To}",
            userId,
            limit,
            offset,
            from,
            to
        );

        IQueryable<Activity> query = this.context.Activities.Where(
            a => a.UserId == userId && !a.IsDeleted
        );

        if (from.HasValue && to.HasValue)
        {
            DateTime rangeStart = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            DateTime rangeEnd = to.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            List<Activity> ranged = await query.Include(a => a.UploadStatuses)
                .Where(a => a.ActivityDate >= rangeStart && a.ActivityDate <= rangeEnd)
                .OrderBy(a => a.ActivityDate)
                .ToListAsync();

            this.logger.LogInformation(
                "Retrieved {Count} activities for user: {UserId} in range {From} to {To}",
                ranged.Count,
                userId,
                from,
                to
            );

            return new PaginatedActivitiesResponse(
                ranged.Select(a => MapToResponse(a)).ToList(),
                ranged.Count,
                ranged.Count,
                0
            );
        }

        int total = await query.CountAsync();

        List<Activity> activities = await query
            .Include(a => a.UploadStatuses)
            .OrderByDescending(a => a.ActivityDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        this.logger.LogInformation(
            "Retrieved {Count} of {Total} activities for user: {UserId}",
            activities.Count,
            total,
            userId
        );

        List<ActivityResponse> items = activities.Select(a => MapToResponse(a)).ToList();

        return new PaginatedActivitiesResponse(items, total, limit, offset);
    }

    public async Task<ActivityResponse> GetActivityByIdAsync(Guid userId, Guid activityId)
    {
        this.logger.LogInformation(
            "Getting activity by ID for user: {UserId}, activity: {ActivityId}",
            userId,
            activityId
        );

        Activity? activity = await this.context.Activities.Include(a => a.UploadStatuses)
            .FirstOrDefaultAsync(a => a.Id == activityId && a.UserId == userId && !a.IsDeleted);

        if (activity == null)
        {
            this.logger.LogWarning(
                "Activity not found - user: {UserId}, activity: {ActivityId}",
                userId,
                activityId
            );
            throw new NotFoundException("Activity not found.");
        }

        this.logger.LogInformation(
            "Activity retrieved successfully - user: {UserId}, activity: {ActivityId}",
            userId,
            activityId
        );

        return MapToResponse(activity);
    }

    public async Task DeleteActivityAsync(Guid userId, Guid activityId)
    {
        this.logger.LogInformation(
            "Soft-deleting activity for user: {UserId}, activity: {ActivityId}",
            userId,
            activityId
        );

        Activity? activity = await this.context.Activities.FirstOrDefaultAsync(
            a => a.Id == activityId && a.UserId == userId && !a.IsDeleted
        );

        if (activity == null)
        {
            this.logger.LogWarning(
                "Activity not found for deletion - user: {UserId}, activity: {ActivityId}",
                userId,
                activityId
            );
            throw new NotFoundException("Activity not found.");
        }

        activity.IsDeleted = true;
        activity.DeletedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Activity soft-deleted successfully - user: {UserId}, activity: {ActivityId}",
            userId,
            activityId
        );
    }

    private static ActivityResponse MapToResponse(Activity a) =>
        new(
            a.Id,
            a.ExternalActivityId,
            a.Source,
            a.OriginalFileName,
            a.FileSizeBytes,
            a.ActivityDate,
            a.ActivityName,
            a.CreatedAt,
            a.UpdatedAt,
            a.UploadStatuses.Select(
                u =>
                    new UploadStatusEntry(
                        u.DestinationServiceType,
                        u.Status,
                        u.LastError,
                        u.RetryCount
                    )
            )
                .ToList(),
            a.Sport,
            a.DurationSeconds,
            a.DistanceMeters,
            a.AvgHeartRate,
            a.AvgPower,
            a.ScheduledWorkoutId
        );
}
