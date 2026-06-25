namespace FitSync.Api.Features.Profile.DTOs;

using System.ComponentModel.DataAnnotations;

public record UpdateUsernameRequest([Required] [MinLength(3)] [MaxLength(50)] string Username);
