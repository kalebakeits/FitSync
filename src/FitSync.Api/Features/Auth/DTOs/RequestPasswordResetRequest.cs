namespace FitSync.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

public class RequestPasswordResetRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
