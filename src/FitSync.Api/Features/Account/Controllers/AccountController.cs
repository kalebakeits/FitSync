namespace FitSync.Api.Features.Account.Controllers;

using FitSync.Api.Features.Account.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController(
    ILogger<AccountController> logger,
    ICurrentUserService currentUserService,
    IAccountService accountService
) : ControllerBase
{
    private readonly ILogger<AccountController> logger = logger;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly IAccountService accountService = accountService;

    [HttpDelete]
    public async Task<ActionResult> Delete()
    {
        this.logger.LogInformation("Account deletion requested...");
        Guid userId = this.currentUserService.GetUserId();
        await this.accountService.DeleteUserAsync(userId);
        this.logger.LogInformation("User {UserId} successfully deleted.", userId);
        return this.Ok();
    }
}
