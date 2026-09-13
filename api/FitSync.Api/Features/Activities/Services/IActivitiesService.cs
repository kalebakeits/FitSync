namespace FitSync.Api.Features.Activities.Services;

using FitSync.Api.Features.Activities.DTOs;

public interface IActivitiesService
{
    Task<PaginatedActivitiesResponse> GetActivitiesAsync(Guid userId, int limit, int offset);
    Task<ActivityResponse> GetActivityByIdAsync(Guid userId, Guid activityId);
    Task DeleteActivityAsync(Guid userId, Guid activityId);
}
