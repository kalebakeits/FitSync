using System.ComponentModel.DataAnnotations;

namespace FitSync.Api.Features.Profile.DTOs;

public class UpdateUsernameRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public required string Username { get; set; }
}
