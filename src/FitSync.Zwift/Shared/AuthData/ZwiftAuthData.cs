namespace FitSync.Zwift.Shared.AuthData;

public class ZwiftAuthData
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? ProfileId { get; set; }
}
