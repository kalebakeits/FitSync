namespace FitSync.Shared.Features.Email.Services;

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class EmailService(IOptions<EmailConfiguration> emailConfig, ILogger<EmailService> logger)
    : IEmailService
{
    private readonly EmailConfiguration emailConfig = emailConfig.Value;
    private readonly ILogger<EmailService> logger = logger;

    public async Task SendVerificationEmailAsync(
        string toEmail,
        string username,
        string verificationToken
    )
    {
        this.logger.LogInformation(
            "Sending verification email to {Email} for user {Username}",
            toEmail,
            username
        );

        string subject = "Verify Your FitSync Account";
        string verificationUrl = $"{this.emailConfig.AppUrl}/verify?token={verificationToken}";
        string body =
            $@"
            <html>
            <body>
                <h2>Welcome to FitSync, {username}!</h2>
                <p>Thank you for registering. Please verify your email address by clicking the link below:</p>
                <p><a href=""{verificationUrl}"">Verify Email Address</a></p>
                <p>Or copy and paste this URL into your browser:</p>
                <p>{verificationUrl}</p>
                <p>This link will expire in 24 hours.</p>
                <p>If you did not create this account, please ignore this email.</p>
            </body>
            </html>";

        await this.SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string username,
        string resetToken
    )
    {
        this.logger.LogInformation(
            "Sending password reset email to {Email} for user {Username}",
            toEmail,
            username
        );

        string subject = "Reset Your FitSync Password";
        string resetUrl = $"{this.emailConfig.AppUrl}/reset-password?token={resetToken}";
        string body =
            $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>Hi {username},</p>
                <p>We received a request to reset your password. Click the link below to reset it:</p>
                <p><a href=""{resetUrl}"">Reset Password</a></p>
                <p>Or copy and paste this URL into your browser:</p>
                <p>{resetUrl}</p>
                <p>This link will expire in 1 hour.</p>
                <p>If you did not request a password reset, please ignore this email and your password will remain unchanged.</p>
            </body>
            </html>";

        await this.SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendEmailChangedNotificationAsync(string toEmail, string username)
    {
        this.logger.LogInformation(
            "Sending email changed notification to {Email} for user {Username}",
            toEmail,
            username
        );

        string subject = "Your FitSync Email Has Been Changed";
        string body =
            $@"
            <html>
            <body>
                <h2>Email Address Changed</h2>
                <p>Hi {username},</p>
                <p>Your FitSync account email address has been changed to this email address.</p>
                <p>Please verify your new email address to continue using all features of FitSync.</p>
            </body>
            </html>";

        await this.SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            using SmtpClient smtpClient =
                new(this.emailConfig.SmtpHost, this.emailConfig.SmtpPort)
                {
                    Credentials = new NetworkCredential(
                        this.emailConfig.SmtpUsername,
                        this.emailConfig.SmtpPassword
                    ),
                    EnableSsl = this.emailConfig.SmtpEnableSsl
                };

            using MailMessage mailMessage =
                new()
                {
                    From = new MailAddress(this.emailConfig.FromEmail, this.emailConfig.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            this.logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}

public class EmailConfiguration
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool SmtpEnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string AppUrl { get; set; } = string.Empty;
}
