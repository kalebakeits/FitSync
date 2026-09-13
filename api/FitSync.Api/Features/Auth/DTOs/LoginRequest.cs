namespace FitSync.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

public record LoginRequest(
    [Required] string Identifier, // Username or Email
    [Required] string Password
);
