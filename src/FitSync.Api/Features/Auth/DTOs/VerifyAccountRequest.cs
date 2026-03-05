namespace FitSync.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

public class VerifyAccountRequest
{
    [Required]
    public required string Token { get; set; }
}
