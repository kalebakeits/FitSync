namespace FitSync.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

public class ConfirmPasswordResetRequest
{
    [Required]
    public required string Token { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public required string NewPassword { get; set; }
}
