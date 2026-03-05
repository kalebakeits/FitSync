namespace FitSync.Garmin.Uploader.Features.GarminUpload.Services;

using FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;
using Flurl.Http;

public interface IGarminApiClient
{
    Task<CookieJar> InitCookieJarAsync(CancellationToken ct);
    Task<string> GetCsrfTokenAsync(CookieJar jar, CancellationToken ct);
    Task<SendCredentialsResult> SendCredentialsAsync(string email, string password, string csrfToken, CookieJar jar, CancellationToken ct);
    Task<ConsumerCredentials> GetConsumerCredentialsAsync(CancellationToken ct);
    Task<(string token, string secret)> GetOAuth1TokenAsync(string ticket, ConsumerCredentials credentials, CancellationToken ct);
    Task<GarminOAuth2Token> GetOAuth2TokenAsync(string oauth1Token, string oauth1Secret, ConsumerCredentials credentials, CancellationToken ct);
    Task<UploadResult> UploadActivityAsync(byte[] fitFileData, string accessToken, CancellationToken ct);
}
