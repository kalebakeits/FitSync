using System.ComponentModel.DataAnnotations;

namespace FitSync.Api.Features.Profile.DTOs;

public class UpdatePasswordRequest
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public required string NewPassword { get; set; }
}
