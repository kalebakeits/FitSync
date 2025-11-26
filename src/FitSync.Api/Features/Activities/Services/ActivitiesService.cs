namespace FitSync.Api.Features.Activities.Services;

using FitSync.Api.Exceptions;
using FitSync.Api.Features.Activities.DTOs;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

public class ActivitiesService(FitSyncDbContext context, ILogger<ActivitiesService> logger)
    : IActivitiesService
{
    private readonly FitSyncDbContext context = context;
    private readonly ILogger<ActivitiesService> logger = logger;

    public async Task<PaginatedActivitiesResponse> GetActivitiesAsync(
        Guid userId,
        ActivityStatus? status,
        int limit,
        int offset
    )
    {
        this.logger.LogInformation(
            "Getting activities for user: {UserId}, status: {Status}, limit: {Limit}, offset: {Offset}",
            userId,
            status,
            limit,
            offset
        );

        IQueryable<Activity> query = this.context.Activities.Where(a => a.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        int total = await query.CountAsync();

        List<Activity> activities = await query
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

        List<ActivityResponse> items = activities
            .Select(
                a =>
                    new ActivityResponse
                    {
                        Id = a.Id,
                        ExternalActivityId = a.ExternalActivityId,
                        Source = a.Source,
                        Status = a.Status,
                        OriginalFileName = a.OriginalFileName,
                        FileSizeBytes = a.FileSizeBytes,
                        ActivityDate = a.ActivityDate,
                        ActivityName = a.ActivityName,
                        RetryCount = a.RetryCount,
                        LastError = a.LastError,
                        LastErrorAt = a.LastErrorAt,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    }
            )
            .ToList();

        return new PaginatedActivitiesResponse
        {
            Items = items,
            Total = total,
            Limit = limit,
            Offset = offset
        };
    }

    public async Task<ActivityResponse> GetActivityByIdAsync(Guid userId, Guid activityId)
    {
        this.logger.LogInformation(
            "Getting activity by ID for user: {UserId}, activity: {ActivityId}",
            userId,
            activityId
        );

        Activity? activity = await this.context.Activities.FirstOrDefaultAsync(
            a => a.Id == activityId && a.UserId == userId
        );

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

        return new ActivityResponse
        {
            Id = activity.Id,
            ExternalActivityId = activity.ExternalActivityId,
            Source = activity.Source,
            Status = activity.Status,
            OriginalFileName = activity.OriginalFileName,
            FileSizeBytes = activity.FileSizeBytes,
            ActivityDate = activity.ActivityDate,
            ActivityName = activity.ActivityName,
            RetryCount = activity.RetryCount,
            LastError = activity.LastError,
            LastErrorAt = activity.LastErrorAt,
            CreatedAt = activity.CreatedAt,
            UpdatedAt = activity.UpdatedAt
        };
    }
}
