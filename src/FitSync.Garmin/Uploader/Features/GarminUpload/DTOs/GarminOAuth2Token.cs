namespace FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;

public class GarminOAuth2Token
{
    public string Access_Token { get; set; } = null!;
    public string Refresh_Token { get; set; } = null!;
    public int Expires_In { get; set; }
}
