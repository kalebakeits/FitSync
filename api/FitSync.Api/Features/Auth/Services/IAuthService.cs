namespace FitSync.Api.Features.Auth.Services;

using FitSync.Api.Features.Auth.DTOs;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task VerifyAccountAsync(string token);
    Task ResendVerificationEmailAsync(string email);
    Task RequestPasswordResetAsync(string email);
    Task ConfirmPasswordResetAsync(string token, string newPassword);
    Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId);
}
