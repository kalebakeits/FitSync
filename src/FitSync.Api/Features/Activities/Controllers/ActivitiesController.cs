namespace FitSync.Api.Features.Activities.Controllers;

using FitSync.Api.Features.Activities.DTOs;
using FitSync.Api.Features.Activities.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ActivitiesController(
    IActivitiesService activitiesService,
    IActivityRetryService activityRetryService,
    ICurrentUserService currentUserService,
    ILogger<ActivitiesController> logger
) : ControllerBase
{
    private readonly IActivitiesService activitiesService = activitiesService;
    private readonly IActivityRetryService activityRetryService = activityRetryService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<ActivitiesController> logger = logger;

    [HttpGet]
    public async Task<ActionResult<PaginatedActivitiesResponse>> GetActivities(
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0
    )
    {
        this.logger.LogInformation(
            "GetActivities called with limit: {Limit}, offset: {Offset}",
            limit,
            offset
        );

        Guid userId = this.currentUserService.GetUserId();
        PaginatedActivitiesResponse response = await this.activitiesService.GetActivitiesAsync(
            userId,
            limit,
            offset
        );

        this.logger.LogInformation(
            "Retrieved {Count} of {Total} activities for user: {UserId}",
            response.Items.Count,
            response.Total,
            userId
        );
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityResponse>> GetActivity(Guid id)
    {
        this.logger.LogInformation("GetActivity called for activity: {ActivityId}", id);

        Guid userId = this.currentUserService.GetUserId();
        ActivityResponse activity = await this.activitiesService.GetActivityByIdAsync(userId, id);

        this.logger.LogInformation(
            "Activity retrieved successfully: {ActivityId} for user: {UserId}",
            id,
            userId
        );
        return Ok(activity);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteActivity(Guid id)
    {
        this.logger.LogInformation("DeleteActivity called for activity: {ActivityId}", id);

        Guid userId = this.currentUserService.GetUserId();
        await this.activitiesService.DeleteActivityAsync(userId, id);

        this.logger.LogInformation(
            "Activity soft-deleted successfully: {ActivityId} for user: {UserId}",
            id,
            userId
        );
        return NoContent();
    }

    [HttpPost("{id}/retry")]
    public async Task<ActionResult> RetryActivity(Guid id, CancellationToken ct)
    {
        this.logger.LogInformation("RetryActivity called for activity: {ActivityId}", id);

        Guid userId = this.currentUserService.GetUserId();
        await this.activityRetryService.RetryFailedAsync(userId, id, ct);

        this.logger.LogInformation(
            "Retry queued for activity: {ActivityId}, user: {UserId}",
            id,
            userId
        );
        return NoContent();
    }

    [HttpPost("{id}/push")]
    public async Task<ActionResult> PushActivity(
        Guid id,
        [FromBody] PushToDestinationRequest request,
        CancellationToken ct
    )
    {
        this.logger.LogInformation(
            "PushActivity called for activity: {ActivityId}, destination: {Destination}",
            id,
            request.DestinationServiceType
        );

        Guid userId = this.currentUserService.GetUserId();
        await this.activityRetryService.PushToDestinationAsync(
            userId,
            id,
            request.DestinationServiceType,
            ct
        );

        this.logger.LogInformation(
            "Activity {ActivityId} pushed to destination {Destination} for user: {UserId}",
            id,
            request.DestinationServiceType,
            userId
        );
        return NoContent();
    }
}
