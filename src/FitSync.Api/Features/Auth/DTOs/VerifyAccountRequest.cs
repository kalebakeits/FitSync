namespace FitSync.Api.Features.Auth.DTOs;

using System.ComponentModel.DataAnnotations;

public record VerifyAccountRequest([Required] string Token);
