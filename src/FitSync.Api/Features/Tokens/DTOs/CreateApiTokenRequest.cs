namespace FitSync.Api.Features.Tokens.DTOs;

using System.ComponentModel.DataAnnotations;

public record CreateApiTokenRequest([Required, MaxLength(255)] string Name);
