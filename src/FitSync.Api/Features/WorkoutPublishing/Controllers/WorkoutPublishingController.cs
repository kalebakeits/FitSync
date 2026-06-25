namespace FitSync.Api.Features.WorkoutPublishing.Controllers;

using FitSync.Api.Features.WorkoutPublishing.DTOs;
using FitSync.Api.Features.WorkoutPublishing.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("workouts/publish/{workoutId}")]
[Authorize]
public class WorkoutPublishingController(
    IWorkoutPublishingService workoutPublishingService,
    ICurrentUserService currentUserService,
    ILogger<WorkoutPublishingController> logger
) : ControllerBase
{
    private readonly IWorkoutPublishingService workoutPublishingService = workoutPublishingService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<WorkoutPublishingController> logger = logger;

    [HttpPost]
    public async Task<IActionResult> PublishWorkout(
        Guid workoutId,
        [FromBody] PublishWorkoutRequest request,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation(
            "PublishWorkout called for workout {WorkoutId} to {ServiceType}.",
            workoutId,
            request.ServiceType
        );

        Guid userId = this.currentUserService.GetUserId();

        await this.workoutPublishingService.PublishAsync(
            userId,
            workoutId,
            request.ServiceType,
            request.ScheduledDate,
            cancellationToken
        );

        return this.NoContent();
    }
}
