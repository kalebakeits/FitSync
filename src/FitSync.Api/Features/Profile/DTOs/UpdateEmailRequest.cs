namespace FitSync.Api.Features.Profile.DTOs;

using System.ComponentModel.DataAnnotations;

public record UpdateEmailRequest([Required] [EmailAddress] string Email);
