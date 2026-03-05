namespace FitSync.Api.Features.Auth.Controllers;

using FitSync.Api.Features.Auth.DTOs;
using FitSync.Api.Features.Auth.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AuthController(
    IAuthService authService,
    ICurrentUserService currentUserService,
    ILogger<AuthController> logger
) : ControllerBase
{
    private readonly IAuthService authService = authService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<AuthController> logger = logger;

    [HttpPost("register")]
    public async Task<ActionResult<AuthSuccessResponse>> Register(
        [FromBody] RegisterRequest request
    )
    {
        this.logger.LogInformation("Register called for username: {Username}", request.Username);

        AuthResponse response = await this.authService.RegisterAsync(request);

        this.logger.LogInformation("User registered successfully: {UserId}", response.UserId);
        return Ok(
            new AuthSuccessResponse { UserId = response.UserId, Username = response.Username }
        );
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthSuccessResponse>> Login([FromBody] LoginRequest request)
    {
        this.logger.LogInformation("Login called for identifier: {Identifier}", request.Identifier);

        AuthResponse response = await this.authService.LoginAsync(request);

        // Set session cookie
        Response.Cookies.Append(
            "FitSync.Session",
            response.SessionId,
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            }
        );

        this.logger.LogInformation("User logged in successfully: {UserId}", response.UserId);
        return Ok(
            new AuthSuccessResponse { UserId = response.UserId, Username = response.Username }
        );
    }

    [HttpPost("verify")]
    public async Task<ActionResult> VerifyAccount([FromBody] VerifyAccountRequest request)
    {
        this.logger.LogInformation("Verify account called");

        await this.authService.VerifyAccountAsync(request.Token);

        this.logger.LogInformation("Account verified successfully");
        return Ok();
    }

    [HttpPost("resend-verification")]
    public async Task<ActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        this.logger.LogInformation("Resend verification called for email: {Email}", request.Email);

        await this.authService.ResendVerificationEmailAsync(request.Email);

        this.logger.LogInformation("Verification email resent for email: {Email}", request.Email);
        return Ok();
    }

    [HttpPost("request-password-reset")]
    public async Task<ActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetRequest request
    )
    {
        this.logger.LogInformation("Password reset requested for email: {Email}", request.Email);

        await this.authService.RequestPasswordResetAsync(request.Email);

        this.logger.LogInformation(
            "Password reset email sent (or user not found) for email: {Email}",
            request.Email
        );

        return Ok();
    }

    [HttpPost("confirm-password-reset")]
    public async Task<ActionResult> ConfirmPasswordReset(
        [FromBody] ConfirmPasswordResetRequest request
    )
    {
        this.logger.LogInformation("Password reset confirmation called");

        await this.authService.ConfirmPasswordResetAsync(request.Token, request.NewPassword);

        this.logger.LogInformation("Password reset successfully");
        return Ok();
    }

    [Authorize]
    [HttpGet("current-user")]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation("Get current user called for userId: {UserId}", userId);

        CurrentUserResponse user = await this.authService.GetCurrentUserAsync(userId);

        return Ok(user);
    }
}
