namespace FitSync.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

public record RegisterRequest(
    [Required] [StringLength(50, MinimumLength = 3)] string Username,
    [Required] [EmailAddress] string Email,
    [Required] [StringLength(100, MinimumLength = 8)] string Password
);
