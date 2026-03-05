namespace FitSync.Api.Features.Wahoo.Services;

public sealed class WahooTokenResult
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }

    public required DateTime ExpiresAtUtc { get; set; }
}
