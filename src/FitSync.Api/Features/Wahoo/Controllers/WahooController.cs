namespace FitSync.Api.Features.Wahoo.Controllers;

using FitSync.Api.Configurations;
using FitSync.Api.Features.Wahoo.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/wahoo")]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class WahooController(
    IWahooConnectionService connectionService,
    ICurrentUserService currentUserService,
    IOptions<WahooOptions> wahooOptions,
    ILogger<WahooController> logger
) : ControllerBase
{
    private readonly IWahooConnectionService connectionService = connectionService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly IOptions<WahooOptions> wahooOptions = wahooOptions;
    private readonly ILogger<WahooController> logger = logger;

    [HttpGet("connect")]
    public ActionResult Connect()
    {
        Guid userId = this.currentUserService.GetUserId();
        string authorizeUrl = this.connectionService.BuildAuthorizeUrl(userId);
        this.logger.LogInformation("Redirecting user {UserId} to Wahoo OAuth authorize endpoint.", userId);
        return this.Redirect(authorizeUrl);
    }

    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<ActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken
    )
    {
        await this.connectionService.CompleteAuthorizationAsync(state, code, cancellationToken);
        this.logger.LogInformation("Wahoo OAuth callback completed.");
        return this.Redirect(this.wahooOptions.Value.FrontendUrl);
    }
}
