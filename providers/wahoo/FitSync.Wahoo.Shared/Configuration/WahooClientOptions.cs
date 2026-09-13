namespace FitSync.Wahoo.Shared.Configuration;

using System.ComponentModel.DataAnnotations;
using FitSync.Shared.Features.RateLimiting;

public class WahooClientOptions
{
    [Required]
    public required string BaseUrl { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }

    public List<RateLimit> RateLimits { get; set; } = [];
}
