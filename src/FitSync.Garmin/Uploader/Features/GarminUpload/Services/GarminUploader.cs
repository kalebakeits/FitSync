namespace FitSync.Garmin.Uploader.Features.GarminUpload.Services;

using System.Net;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.AuthData;
using FitSync.Garmin.Shared.GarminClient.DTOs;
using FitSync.Garmin.Shared.GarminClient.Services;
using FitSync.Garmin.Uploader.Configuration;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Shared.Features.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class GarminUploader(
    ILogger<GarminUploader> logger,
    FitSyncDbContext fitSyncDbContext,
    IEncryptionService encryptionService,
    IGarminApiClient apiClient,
    IGarminAuthService authService,
    IRateLimiter rateLimiter,
    IOptions<GarminUploaderOptions> options
) : IGarminUploader
{
    private readonly ILogger<GarminUploader> logger = logger;
    private readonly FitSyncDbContext fitSyncDbContext = fitSyncDbContext;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly IGarminApiClient apiClient = apiClient;
    private readonly IGarminAuthService authService = authService;
    private readonly IRateLimiter rateLimiter = rateLimiter;
    private readonly IOptions<GarminUploaderOptions> options = options;

    public async Task<UploadResult> UploadActivityAsync(
        byte[] fitFileData,
        User user,
        CancellationToken cancellationToken = default
    )
    {
        Integration? integration = await this.fitSyncDbContext.Integrations.FirstOrDefaultAsync(
            i => i.UserId == user.Id && i.ServiceType == ServiceTypes.Garmin,
            cancellationToken
        );

        if (integration == null)
        {
            this.logger.LogError("No Garmin integration found for user {UserId}.", user.Id);
            return UploadResult.Failed("No Garmin integration found.");
        }

        try
        {
            await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Garmin auth failed for user {UserId}.", user.Id);
            return UploadResult.Failed($"Authentication failed: {ex.Message}");
        }

        if (
            await this.rateLimiter.RateLimitedReachedAsync(
                ServiceType.GarminUploader,
                this.options.Value.RateLimits,
                cancellationToken
            )
        )
            return UploadResult.RateLimited();

        GarminAuthData authData = integration.GetAuthData<GarminAuthData>(this.encryptionService);
        UploadResult result = await this.apiClient.UploadActivityAsync(
            fitFileData,
            authData.OAuth2AccessToken!,
            cancellationToken
        );

        if (!result.Success && result.StatusCode == HttpStatusCode.Unauthorized)
        {
            this.logger.LogWarning(
                "Garmin upload got 401 for user {UserId}, attempting token refresh.",
                user.Id
            );
            bool refreshed = await this.authService.TryRefreshAsync(integration, cancellationToken);

            if (!refreshed)
            {
                this.logger.LogError("Token refresh failed for user {UserId}.", user.Id);
                return UploadResult.Failed(
                    "Token refresh failed after 401.",
                    HttpStatusCode.Unauthorized
                );
            }

            await this.fitSyncDbContext.Entry(integration).ReloadAsync(cancellationToken);
            authData = integration.GetAuthData<GarminAuthData>(this.encryptionService);
            result = await this.apiClient.UploadActivityAsync(
                fitFileData,
                authData.OAuth2AccessToken!,
                cancellationToken
            );
        }

        if (result.Success)
            this.logger.LogInformation("Upload successful for user {Username}.", user.Username);

        return result;
    }
}
