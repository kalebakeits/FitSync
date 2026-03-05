namespace FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;

using System.Text.Json.Serialization;

public class GarminOAuth2Token
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
