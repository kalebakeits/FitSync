namespace FitSync.Api.Features.WorkoutPublishing.Controllers;

using FitSync.Api.Features.WorkoutPublishing.DTOs;
using FitSync.Api.Features.WorkoutPublishing.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("scheduled-workouts")]
[Authorize]
public class ScheduledWorkoutsController(
    IWorkoutPublishingService workoutPublishingService,
    ICurrentUserService currentUserService,
    ILogger<ScheduledWorkoutsController> logger
) : ControllerBase
{
    private readonly IWorkoutPublishingService workoutPublishingService = workoutPublishingService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<ScheduledWorkoutsController> logger = logger;

    [HttpGet]
    public async Task<ActionResult<List<ScheduledWorkoutResponse>>> GetScheduledWorkouts(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("GetScheduledWorkouts for user {UserId}.", userId);
        List<ScheduledWorkoutResponse> workouts =
            await this.workoutPublishingService.GetScheduledWorkoutsAsync(
                userId,
                from,
                to,
                cancellationToken
            );
        return this.Ok(workouts);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ScheduledWorkoutResponse>> MoveScheduledWorkout(
        Guid id,
        [FromBody] MoveScheduledWorkoutRequest request,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("MoveScheduledWorkout {Id} for user {UserId}.", id, userId);
        ScheduledWorkoutResponse workout =
            await this.workoutPublishingService.MoveScheduledWorkoutAsync(
                userId,
                id,
                request.ScheduledDate,
                cancellationToken
            );
        return this.Ok(workout);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScheduledWorkout(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("DeleteScheduledWorkout {Id} for user {UserId}.", id, userId);
        await this.workoutPublishingService.DeleteScheduledWorkoutAsync(
            userId,
            id,
            cancellationToken
        );
        return this.NoContent();
    }
}
