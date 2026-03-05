namespace FitSync.Api.Features.Credentials.DTOs;

using System.ComponentModel.DataAnnotations;

public class CreateCredentialRequest
{
    [Required]
    public required string ServiceType { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
