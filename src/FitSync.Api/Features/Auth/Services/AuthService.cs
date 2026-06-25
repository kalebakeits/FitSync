namespace FitSync.Api.Features.Auth.Services;

using System.Security.Cryptography;
using FitSync.Api.Exceptions;
using FitSync.Api.Features.Auth.DTOs;
using FitSync.Api.Helpers;
using FitSync.Api.Services;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Email.Services;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using Microsoft.EntityFrameworkCore;

public class AuthService(
    FitSyncDbContext context,
    ISessionService sessionService,
    IEncryptionService encryptionService,
    IEmailService emailService,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly FitSyncDbContext context = context;
    private readonly ISessionService sessionService = sessionService;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly IEmailService emailService = emailService;
    private readonly ILogger<AuthService> logger = logger;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        this.logger.LogInformation(
            "Registering new user with username: {Username}",
            request.Username
        );

        // Validate username has no special characters
        if (!UsernameValidator.IsValid(request.Username))
        {
            this.logger.LogWarning(
                "Username contains special characters: {Username}",
                request.Username
            );
            throw new BadRequestException(
                "Username contains special characters. Only letters, numbers, and underscores are allowed."
            );
        }

        // Check if username already exists
        if (await this.context.Users.AnyAsync(u => u.Username == request.Username))
        {
            this.logger.LogWarning("Username already exists: {Username}", request.Username);
            throw new ConflictException("Username already exists.");
        }

        if (await this.context.Users.AnyAsync(u => u.EmailHash == request.Email.SHA256Hashed()))
        {
            this.logger.LogWarning("Email already exists during registration");
            throw new ConflictException("Email already exists.");
        }

        // Generate verification token
        string verificationToken = GenerateSecureToken();

        // Create new user
        User user =
            new()
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                EmailHash = request.Email.SHA256Hashed(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsVerified = false,
                VerificationToken = verificationToken,
                VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
            };

        // Encrypt email before saving
        user.Encrypt(this.encryptionService);

        this.context.Users.Add(user);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "User registered successfully: {UserId}, username: {Username}",
            user.Id,
            user.Username
        );

        // Send verification email (decrypt email first)
        string emailAddress = user.Decrypt(this.encryptionService);
        await this.emailService.SendVerificationEmailAsync(
            emailAddress,
            user.Username,
            verificationToken
        );

        // Don't create session for unverified users - they need to verify first
        return new AuthResponse(string.Empty, user.Id, user.Username);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        this.logger.LogInformation(
            "Login attempt for identifier: {Identifier}",
            request.Identifier
        );

        User? user = null;

        // Check if identifier is email (contains @) or username
        if (request.Identifier.Contains('@'))
        {
            user = await this.context.Users.FirstOrDefaultAsync(
                u => u.EmailHash == request.Identifier.SHA256Hashed()
            );
        }
        else
        {
            // Search by username
            user = await this.context.Users.FirstOrDefaultAsync(
                u => u.Username == request.Identifier
            );
        }

        if (user == null)
        {
            this.logger.LogWarning(
                "Login failed - user not found: {Identifier}",
                request.Identifier
            );
            throw new NotFoundException("Invalid username or email.");
        }

        // Check if user is verified
        if (!user.IsVerified)
        {
            this.logger.LogWarning(
                "Login failed - account not verified: {UserId}, username: {Username}",
                user.Id,
                user.Username
            );
            throw new ForbiddenException(
                "Account not verified. Please check your email for verification link."
            );
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            this.logger.LogWarning(
                "Login failed - invalid password for identifier: {Identifier}",
                request.Identifier
            );
            throw new NotFoundException("Invalid password.");
        }

        this.logger.LogInformation(
            "User logged in successfully: {UserId}, username: {Username}",
            user.Id,
            user.Username
        );

        // Create session
        string sessionId = await this.sessionService.CreateSessionAsync(user.Id);

        return new AuthResponse(sessionId, user.Id, user.Username);
    }

    public async Task VerifyAccountAsync(string token)
    {
        this.logger.LogInformation("Verifying account with token");

        User? user = await this.context.Users.FirstOrDefaultAsync(
            u => u.VerificationToken == token && u.VerificationTokenExpiresAt > DateTime.UtcNow
        );

        if (user == null)
        {
            this.logger.LogWarning("Verification failed - invalid or expired token");
            throw new BadRequestException("Invalid or expired verification token.");
        }

        user.IsVerified = true;
        user.VerificationToken = null;
        user.VerificationTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Account verified successfully: {UserId}, username: {Username}",
            user.Id,
            user.Username
        );
    }

    public async Task ResendVerificationEmailAsync(string email)
    {
        this.logger.LogInformation("Resending verification email to: {Email}", email);
        User? user = await this.context.Users.FirstOrDefaultAsync(
            u => u.EmailHash == email.SHA256Hashed()
        );

        if (user == null)
        {
            this.logger.LogWarning("User not found for email: {Email}", email);
            throw new BadRequestException("Account not found or already verified.");
        }

        if (user.IsVerified)
        {
            this.logger.LogWarning(
                "Account already verified: {UserId}, username: {Username}",
                user.Id,
                user.Username
            );
            throw new BadRequestException("Account not found or already verified.");
        }

        // Generate new verification token
        string verificationToken = GenerateSecureToken();
        user.VerificationToken = verificationToken;
        user.VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        // Send verification email
        string emailAddress = user.Decrypt(this.encryptionService);
        await this.emailService.SendVerificationEmailAsync(
            emailAddress,
            user.Username,
            verificationToken
        );

        this.logger.LogInformation(
            "Verification email resent: {UserId}, username: {Username}",
            user.Id,
            user.Username
        );
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        this.logger.LogInformation("Password reset requested for email: {Email}", email);
        User? user = await this.context.Users.FirstOrDefaultAsync(
            u => u.Email == email.SHA256Hashed()
        );

        if (user == null)
        {
            this.logger.LogWarning("User not found for email: {Email}", email);
            return;
        }

        // Generate reset token
        string resetToken = GenerateSecureToken();
        user.ResetToken = resetToken;
        user.ResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        // Send reset email
        string emailAddress = user.Decrypt(this.encryptionService);
        await this.emailService.SendPasswordResetEmailAsync(
            emailAddress,
            user.Username,
            resetToken
        );

        this.logger.LogInformation(
            "Password reset email sent: {UserId}, username: {Username}",
            user.Id,
            user.Username
        );
    }

    public async Task ConfirmPasswordResetAsync(string token, string newPassword)
    {
        this.logger.LogInformation("Confirming password reset with token");

        User? user = await this.context.Users.FirstOrDefaultAsync(
            u => u.ResetToken == token && u.ResetTokenExpiresAt > DateTime.UtcNow
        );

        if (user == null)
        {
            this.logger.LogWarning("Password reset failed - invalid or expired token");
            throw new BadRequestException("Invalid or expired reset token.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Password reset successfully: {UserId}, username: {Username}",
            user.Id,
            user.Username
        );
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId)
    {
        this.logger.LogInformation("Getting current user: {UserId}", userId);

        User? user = await this.context.Users.FindAsync(userId);

        if (user == null)
        {
            this.logger.LogWarning("User not found: {UserId}", userId);
            throw new NotFoundException("User not found.");
        }

        string email = user.Decrypt(this.encryptionService);

        return new CurrentUserResponse(
            user.Id,
            user.Username,
            email,
            user.IsVerified,
            user.IsVerified // For now, same as IsVerified
        );
    }

    private static string GenerateSecureToken()
    {
        byte[] tokenBytes = new byte[32];
        RandomNumberGenerator.Fill(tokenBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
