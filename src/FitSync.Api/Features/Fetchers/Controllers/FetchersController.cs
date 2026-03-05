namespace FitSync.Api.Features.Fetchers.Controllers;

using FitSync.Api.Features.Fetchers.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FetchersController(
    IFetchersService fetchersService,
    ICurrentUserService currentUserService,
    ILogger<FetchersController> logger
) : ControllerBase
{
    private readonly IFetchersService fetchersService = fetchersService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<FetchersController> logger = logger;

    [HttpPost("trigger")]
    public async Task<ActionResult> TriggerFetch()
    {
        this.logger.LogInformation("TriggerFetch called");

        Guid userId = this.currentUserService.GetUserId();
        await this.fetchersService.TriggerFetchAsync(userId);

        this.logger.LogInformation("Fetch triggered successfully for user: {UserId}", userId);
        return Ok();
    }
}
