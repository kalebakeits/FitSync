namespace FitSync.Api.Features.TrainingProfile.Controllers;

using FitSync.Api.Features.TrainingProfile.DTOs;
using FitSync.Api.Features.TrainingProfile.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("training-profile")]
public class TrainingProfileController(
    ITrainingProfileService trainingProfileService,
    ICurrentUserService currentUserService,
    ILogger<TrainingProfileController> logger
) : ControllerBase
{
    private readonly ITrainingProfileService trainingProfileService = trainingProfileService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<TrainingProfileController> logger = logger;

    [HttpGet]
    public async Task<ActionResult<TrainingProfileResponse>> GetProfile()
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("GetTrainingProfile called for user: {UserId}", userId);
        TrainingProfileResponse? profile = await this.trainingProfileService.GetProfileAsync(
            userId
        );
        return profile is null ? this.NoContent() : this.Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<TrainingProfileResponse>> UpsertProfile(
        [FromBody] UpsertTrainingProfileRequest request
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("UpsertTrainingProfile called for user: {UserId}", userId);
        TrainingProfileResponse profile = await this.trainingProfileService.UpsertProfileAsync(
            userId,
            request
        );
        return this.Ok(profile);
    }
}
