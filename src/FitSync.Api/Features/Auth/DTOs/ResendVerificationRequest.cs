namespace FitSync.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

public record ResendVerificationRequest([Required] [EmailAddress] string Email);
