namespace FitSync.Api.Features.Profile.DTOs;

using System.ComponentModel.DataAnnotations;

public record UpdatePasswordRequest(
    [Required] string CurrentPassword,
    [Required] [MinLength(8)] [MaxLength(100)] string NewPassword
);
