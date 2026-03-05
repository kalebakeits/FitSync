namespace FitSync.Api.Features.Profile.DTOs;

using System.ComponentModel.DataAnnotations;

public class UpdateEmailRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
