using System.ComponentModel.DataAnnotations;

namespace FitSync.Api.Features.Auth.DTOs;

public class LoginRequest
{
    [Required]
    public required string Identifier { get; set; } // Username or Email

    [Required]
    public required string Password { get; set; }
}
