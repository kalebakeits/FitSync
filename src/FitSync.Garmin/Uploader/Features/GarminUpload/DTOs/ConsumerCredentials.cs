namespace FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;

using System.Text.Json.Serialization;

public class ConsumerCredentials
{
    [JsonPropertyName("consumer_key")]
    public string ConsumerKey { get; set; } = null!;

    [JsonPropertyName("consumer_secret")]
    public string ConsumerSecret { get; set; } = null!;
}
