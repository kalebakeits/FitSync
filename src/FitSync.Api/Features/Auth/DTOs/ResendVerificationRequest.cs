using System.ComponentModel.DataAnnotations;

namespace FitSync.Api.Features.Auth.DTOs;

public class ResendVerificationRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
