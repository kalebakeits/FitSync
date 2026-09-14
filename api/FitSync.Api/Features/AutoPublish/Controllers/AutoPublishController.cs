namespace FitSync.Api.Features.AutoPublish.Controllers;

using FitSync.Api.Features.AutoPublish.DTOs;
using FitSync.Api.Features.AutoPublish.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auto-publish")]
[Authorize]
public class AutoPublishController(
    IAutoPublishSettingsService autoPublishSettingsService,
    ICurrentUserService currentUserService,
    ILogger<AutoPublishController> logger
) : ControllerBase
{
    private readonly IAutoPublishSettingsService autoPublishSettingsService =
        autoPublishSettingsService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<AutoPublishController> logger = logger;

    [HttpGet]
    public async Task<ActionResult<List<AutoPublishSettingResponse>>> GetSettingsAsync(
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("GetSettings called for user {UserId}.", userId);

        List<AutoPublishSettingResponse> settings =
            await this.autoPublishSettingsService.GetSettingsAsync(userId, cancellationToken);

        this.logger.LogInformation(
            "Returning {Count} auto-publish settings for user {UserId}.",
            settings.Count,
            userId
        );
        return this.Ok(settings);
    }

    [HttpPut]
    public async Task<ActionResult> ReplaceSettingsAsync(
        [FromBody] AutoPublishSettingsRequest request,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation(
            "ReplaceSettings called for user {UserId} with {Count} entries.",
            userId,
            request.Settings.Count
        );

        await this.autoPublishSettingsService.ReplaceSettingsAsync(
            userId,
            request,
            cancellationToken
        );

        this.logger.LogInformation("Replaced auto-publish settings for user {UserId}.", userId);
        return this.NoContent();
    }
}
