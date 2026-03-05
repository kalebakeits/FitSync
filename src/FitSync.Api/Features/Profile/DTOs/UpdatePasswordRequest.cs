namespace FitSync.Api.Features.Profile.DTOs;

using System.ComponentModel.DataAnnotations;

public class UpdatePasswordRequest
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public required string NewPassword { get; set; }
}
