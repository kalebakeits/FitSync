namespace FitSync.Api.Features.Profile.Controllers;

using FitSync.Api.Features.Profile.DTOs;
using FitSync.Api.Features.Profile.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController(
    IProfileService profileService,
    ICurrentUserService currentUserService,
    ILogger<ProfileController> logger
) : ControllerBase
{
    private readonly IProfileService profileService = profileService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<ProfileController> logger = logger;

    [HttpPut("username")]
    public async Task<ActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("Update username called for userId: {UserId}", userId);

        await this.profileService.UpdateUsernameAsync(userId, request.Username);

        this.logger.LogInformation("Username updated successfully for userId: {UserId}", userId);
        return Ok();
    }

    [HttpPut("email")]
    public async Task<ActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("Update email called for userId: {UserId}", userId);

        await this.profileService.UpdateEmailAsync(userId, request.Email);

        this.logger.LogInformation("Email updated successfully for userId: {UserId}", userId);
        return Ok();
    }

    [HttpPut("password")]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("Update password called for userId: {UserId}", userId);

        await this.profileService.UpdatePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword
        );

        this.logger.LogInformation("Password updated successfully for userId: {UserId}", userId);
        return Ok();
    }
}
