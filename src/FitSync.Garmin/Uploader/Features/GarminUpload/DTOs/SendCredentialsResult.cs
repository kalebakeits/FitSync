namespace FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;

public class SendCredentialsResult
{
    public bool WasRedirected { get; set; }
    public string RedirectedTo { get; set; } = string.Empty;
    public string RawResponseBody { get; set; } = string.Empty;
}
