namespace FitSync.Api.Features.Workouts.Controllers;

using FitSync.Api.Features.Workouts.DTOs;
using FitSync.Api.Features.Workouts.Services;
using FitSync.Api.Services;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[Authorize]
public class WorkoutsController(
    IWorkoutsService workoutsService,
    ICurrentUserService currentUserService,
    ILogger<WorkoutsController> logger
) : ControllerBase
{
    private readonly IWorkoutsService workoutsService = workoutsService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<WorkoutsController> logger = logger;

    [HttpGet]
    public async Task<ActionResult<PaginatedWorkoutsResponse>> GetWorkouts(
        [FromQuery] string? search,
        [FromQuery] List<string>? tags,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0
    )
    {
        this.logger.LogInformation(
            "GetWorkouts called with search: {Search}, tags: {Tags}",
            search,
            tags
        );
        Guid userId = this.currentUserService.GetUserId();
        PaginatedWorkoutsResponse response = await this.workoutsService.GetWorkoutsAsync(
            userId,
            search,
            tags,
            limit,
            offset
        );
        return this.Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkoutResponse>> GetWorkout(Guid id)
    {
        this.logger.LogInformation("GetWorkout called for workout: {WorkoutId}", id);
        Guid userId = this.currentUserService.GetUserId();
        WorkoutResponse workout = await this.workoutsService.GetWorkoutByIdAsync(userId, id);
        return this.Ok(workout);
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutResponse>> CreateWorkout([FromBody] WorkoutSchema schema)
    {
        this.logger.LogInformation("Recieved request to generate workout file {@schema}", schema);
        Guid userId = this.currentUserService.GetUserId();
        WorkoutResponse workout = await this.workoutsService.CreateWorkoutAsync(userId, schema);
        return this.CreatedAtAction(nameof(this.GetWorkout), new { id = workout.Id }, workout);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkoutResponse>> UpdateWorkout(
        Guid id,
        [FromBody] UpdateWorkoutRequest request
    )
    {
        this.logger.LogInformation("UpdateWorkout called for workout: {WorkoutId}", id);
        Guid userId = this.currentUserService.GetUserId();
        WorkoutResponse workout = await this.workoutsService.UpdateWorkoutAsync(
            userId,
            id,
            request
        );
        return this.Ok(workout);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorkout(Guid id)
    {
        this.logger.LogInformation("DeleteWorkout called for workout: {WorkoutId}", id);
        Guid userId = this.currentUserService.GetUserId();
        await this.workoutsService.DeleteWorkoutAsync(userId, id);
        return this.NoContent();
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadWorkout(Guid id)
    {
        this.logger.LogInformation("DownloadWorkout called for workout: {WorkoutId}", id);
        Guid userId = this.currentUserService.GetUserId();
        WorkoutResponse workout = await this.workoutsService.GetWorkoutByIdAsync(userId, id);
        byte[] file = await this.workoutsService.DownloadWorkoutAsync(userId, id);
        return this.File(file, "application/octet-stream", $"{workout.Name}.fit");
    }
}
