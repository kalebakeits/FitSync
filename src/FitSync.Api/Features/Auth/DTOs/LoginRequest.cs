namespace FitSync.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
    [Required]
    public required string Identifier { get; set; } // Username or Email

    [Required]
    public required string Password { get; set; }
}
