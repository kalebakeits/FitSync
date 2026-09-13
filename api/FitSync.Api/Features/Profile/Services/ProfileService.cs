namespace FitSync.Api.Features.Profile.Services;

using FitSync.Api.Exceptions;
using FitSync.Api.Helpers;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Email.Services;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using Microsoft.EntityFrameworkCore;

public interface IProfileService
{
    Task UpdateUsernameAsync(Guid userId, string newUsername);
    Task UpdateEmailAsync(Guid userId, string newEmail);
    Task UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword);
}

public class ProfileService(
    FitSyncDbContext context,
    IEncryptionService encryptionService,
    IEmailService emailService,
    ILogger<ProfileService> logger
) : IProfileService
{
    private readonly FitSyncDbContext context = context;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly IEmailService emailService = emailService;
    private readonly ILogger<ProfileService> logger = logger;

    public async Task UpdateUsernameAsync(Guid userId, string newUsername)
    {
        this.logger.LogInformation(
            "Updating username for user: {UserId} to {NewUsername}",
            userId,
            newUsername
        );

        // Validate username has no special characters
        if (!UsernameValidator.IsValid(newUsername))
        {
            this.logger.LogWarning(
                "Username update failed - contains special characters: {NewUsername}",
                newUsername
            );
            throw new BadRequestException(
                "Username contains special characters. Only letters, numbers, and underscores are allowed."
            );
        }

        // Check if username already exists
        if (await this.context.Users.AnyAsync(u => u.Username == newUsername && u.Id != userId))
        {
            this.logger.LogWarning(
                "Username update failed - already exists: {NewUsername}",
                newUsername
            );
            throw new ConflictException("Username already exists.");
        }

        User? user = await this.context.Users.FindAsync(userId);

        if (user == null)
        {
            this.logger.LogWarning("Username update failed - user not found: {UserId}", userId);
            throw new NotFoundException("User not found.");
        }

        user.Username = newUsername;
        user.UpdatedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation("Username updated successfully for user: {UserId}", userId);
    }

    public async Task UpdateEmailAsync(Guid userId, string newEmail)
    {
        this.logger.LogInformation(
            "Updating email for user: {UserId} to {NewEmail}",
            userId,
            newEmail
        );
        string emailHash = newEmail.SHA256Hashed();
        if (await this.context.Users.AnyAsync(u => u.Id != userId && u.EmailHash == emailHash))
        {
            this.logger.LogWarning("Email already exists during registration");
            throw new ConflictException("Email already exists.");
        }

        User? user = await this.context.Users.FindAsync(userId);

        if (user == null)
        {
            this.logger.LogWarning("Email update failed - user not found: {UserId}", userId);
            throw new NotFoundException("User not found.");
        }

        // Decrypt to get current email before update
        user.Decrypt(this.encryptionService);
        string oldEmail = user.Email;

        // Update email
        user.Email = newEmail;
        user.EmailHash = emailHash;
        user.UpdatedAt = DateTime.UtcNow;

        // For email changes, we keep IsVerified = true but user needs to verify new email
        // Generate verification token for new email
        string verificationToken = this.GenerateSecureToken();
        user.VerificationToken = verificationToken;
        user.VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);

        // Encrypt new email before saving
        user.Encrypt(this.encryptionService);

        await this.context.SaveChangesAsync();

        // Send notification to new email
        await this.emailService.SendEmailChangedNotificationAsync(newEmail, user.Username);

        // Send verification email to new email
        await this.emailService.SendVerificationEmailAsync(
            newEmail,
            user.Username,
            verificationToken
        );

        this.logger.LogInformation("Email updated successfully for user: {UserId}", userId);
    }

    public async Task UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        this.logger.LogInformation("Updating password for user: {UserId}", userId);

        User? user = await this.context.Users.FindAsync(userId);

        if (user == null)
        {
            this.logger.LogWarning("Password update failed - user not found: {UserId}", userId);
            throw new NotFoundException("User not found.");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            this.logger.LogWarning(
                "Password update failed - invalid current password for user: {UserId}",
                userId
            );
            throw new BadRequestException("Current password is incorrect.");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation("Password updated successfully for user: {UserId}", userId);
    }

    private string GenerateSecureToken()
    {
        byte[] tokenBytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(tokenBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
