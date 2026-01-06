namespace FitSync.Api.Features.Activities.Controllers;

using FitSync.Api.Features.Activities.DTOs;
using FitSync.Api.Features.Activities.Services;
using FitSync.Api.Services;
using FitSync.Database.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivitiesController(
    IActivitiesService activitiesService,
    ICurrentUserService currentUserService,
    ILogger<ActivitiesController> logger
) : ControllerBase
{
    private readonly IActivitiesService activitiesService = activitiesService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<ActivitiesController> logger = logger;

    [HttpGet]
    public async Task<ActionResult<PaginatedActivitiesResponse>> GetActivities(
        [FromQuery] ActivityStatus? status = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0
    )
    {
        this.logger.LogInformation(
            "GetActivities called with status: {Status}, limit: {Limit}, offset: {Offset}",
            status,
            limit,
            offset
        );

        Guid userId = this.currentUserService.GetUserId();
        PaginatedActivitiesResponse response = await this.activitiesService.GetActivitiesAsync(
            userId,
            status,
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
            "Activity deleted successfully: {ActivityId} for user: {UserId}",
            id,
            userId
        );
        return NoContent();
    }
}
