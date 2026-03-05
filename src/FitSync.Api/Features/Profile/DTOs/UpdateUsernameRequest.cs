namespace FitSync.Api.Features.Profile.DTOs;

using System.ComponentModel.DataAnnotations;

public class UpdateUsernameRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public required string Username { get; set; }
}
