namespace FitSync.Garmin.Shared.AuthData;

public class GarminAuthData
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? OAuth1Token { get; set; }
    public string? OAuth1TokenSecret { get; set; }
    public string? OAuth2AccessToken { get; set; }
    public DateTime? OAuth2ExpiresAt { get; set; }

    public bool HasValidOAuth2Token() =>
        OAuth2AccessToken != null && OAuth2ExpiresAt > DateTime.UtcNow.AddMinutes(5);

    public bool HasOAuth1Token() => OAuth1Token != null && OAuth1TokenSecret != null;
}
