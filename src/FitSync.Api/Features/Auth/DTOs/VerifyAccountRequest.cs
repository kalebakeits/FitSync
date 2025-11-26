using System.ComponentModel.DataAnnotations;

namespace FitSync.Api.Features.Auth.DTOs;

public class VerifyAccountRequest
{
    [Required]
    public required string Token { get; set; }
}
