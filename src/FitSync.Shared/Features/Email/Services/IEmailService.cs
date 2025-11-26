namespace FitSync.Shared.Features.Email.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string username, string verificationToken);
    Task SendPasswordResetEmailAsync(string toEmail, string username, string resetToken);
    Task SendEmailChangedNotificationAsync(string toEmail, string username);
}
