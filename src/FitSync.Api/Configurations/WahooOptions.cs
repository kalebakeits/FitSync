namespace FitSync.Api.Configurations;

using System.ComponentModel.DataAnnotations;

public class WahooOptions
{
    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }

    [Required]
    public required string RedirectUri { get; set; }

    [Required]
    public required string WebhookToken { get; set; }

    [Required]
    public required string BaseUrl { get; set; }

    [Required]
    public required string FrontendUrl { get; set; }
}
