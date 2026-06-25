namespace FitSync.Api.Features.Credentials.DTOs;

using System.ComponentModel.DataAnnotations;

public record CreateCredentialRequest(
    [Required] string ServiceType,
    [Required] string Username,
    [Required] string Password
);
