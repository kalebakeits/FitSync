using System.ComponentModel.DataAnnotations;

namespace FitSync.Api.Features.Auth.DTOs;

public class RequestPasswordResetRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
