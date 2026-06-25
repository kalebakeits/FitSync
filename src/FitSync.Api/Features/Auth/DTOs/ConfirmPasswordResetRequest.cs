namespace FitSync.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

public record ConfirmPasswordResetRequest(
    [Required] string Token,
    [Required] [MinLength(8)] [MaxLength(100)] string NewPassword
);
