using System.ComponentModel.DataAnnotations;

namespace FitSync.Api.Features.Profile.DTOs;

public class UpdateEmailRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
