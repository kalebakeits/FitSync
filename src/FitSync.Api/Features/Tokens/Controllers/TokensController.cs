namespace FitSync.Api.Features.Tokens.Controllers;

using FitSync.Api.Features.Tokens.DTOs;
using FitSync.Api.Features.Tokens.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("tokens")]
[Authorize]
public class TokensController(
    IApiTokenService apiTokenService,
    ICurrentUserService currentUserService,
    ILogger<TokensController> logger
) : ControllerBase
{
    private readonly IApiTokenService apiTokenService = apiTokenService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<TokensController> logger = logger;

    [HttpGet]
    public async Task<ActionResult<List<ApiTokenResponse>>> GetTokens()
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("GetTokens called for user {UserId}.", userId);
        List<ApiTokenResponse> tokens = await this.apiTokenService.GetTokensAsync(userId);
        return this.Ok(tokens);
    }

    [HttpPost]
    public async Task<ActionResult<CreateApiTokenResponse>> CreateToken(
        [FromBody] CreateApiTokenRequest request
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("CreateToken called for user {UserId}.", userId);
        CreateApiTokenResponse token = await this.apiTokenService.CreateTokenAsync(
            userId,
            request.Name
        );
        return this.CreatedAtAction(nameof(this.GetTokens), token);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RevokeToken(Guid id)
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation(
            "RevokeToken called for token {TokenId}, user {UserId}.",
            id,
            userId
        );
        await this.apiTokenService.RevokeTokenAsync(userId, id);
        return this.NoContent();
    }
}
