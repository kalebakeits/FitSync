namespace FitSync.Wahoo.Shared.AuthData;

public class WahooAuthData
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    public long WahooUserId { get; set; }
}
