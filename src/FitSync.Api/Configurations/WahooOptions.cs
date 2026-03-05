namespace FitSync.Api.Configurations;

using System.ComponentModel.DataAnnotations;
using FitSync.Wahoo.Shared.Configuration;

public class WahooOptions : WahooClientOptions
{
    [Required]
    public required string RedirectUri { get; set; }

    [Required]
    public required string WebhookToken { get; set; }

    [Required]
    public required string FrontendUrl { get; set; }
}
