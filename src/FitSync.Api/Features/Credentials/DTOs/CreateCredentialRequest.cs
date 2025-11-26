using System.ComponentModel.DataAnnotations;

namespace FitSync.Api.Features.Credentials.DTOs;

public class CreateCredentialRequest
{
    [Required]
    public required string ServiceType { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
