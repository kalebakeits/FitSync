namespace FitSync.Garmin.Uploader.Features.GarminUpload.Services;

using System.Net;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.AuthData;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;
using global::Garmin.Connect;
using global::Garmin.Connect.Auth;
using global::Garmin.Connect.Auth.External;
using global::Garmin.Connect.Exceptions;
using Microsoft.EntityFrameworkCore;

public class GarminUploader(
    ILogger<GarminUploader> logger,
    FitSyncDbContext fitSyncDbContext,
    IEncryptionService encryptionService
) : IGarminUploader
{
    private readonly ILogger<GarminUploader> logger = logger;
    private readonly FitSyncDbContext fitSyncDbContext = fitSyncDbContext;
    private readonly IEncryptionService encryptionService = encryptionService;

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

        GarminAuthData authData = integration.GetAuthData<GarminAuthData>(this.encryptionService);
        BasicAuthParameters authParameters = new(authData.Username, authData.Password);
        GarminConnectClient client = new(new GarminConnectContext(new HttpClient(), authParameters));

        string tempFile = Path.Combine(Path.GetTempPath(), $"fit_{Guid.NewGuid()}.fit");
        await File.WriteAllBytesAsync(tempFile, fitFileData, cancellationToken);

        try
        {
            await client.UploadFile(tempFile, cancellationToken);
            this.logger.LogInformation("Upload successful for user {Username}.", user.Username);
            return UploadResult.Succeeded();
        }
        catch (GarminConnectRequestException ex)
        {
            return this.HandleException(ex, ex.Status, user.Id);
        }
        catch (GarminConnectAuthenticationException ex)
        {
            return this.HandleException(ex, HttpStatusCode.Unauthorized, user.Id);
        }
        catch (HttpRequestException ex)
        {
            return this.HandleException(ex, ex.StatusCode, user.Id);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error during Garmin upload for user {UserId}.", user.Id);
            return UploadResult.Failed($"Unexpected error: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private UploadResult HandleException(Exception ex, HttpStatusCode? statusCode, Guid userId)
    {
        if (statusCode.HasValue && (int)statusCode >= 200 && (int)statusCode <= 299)
            return UploadResult.Succeeded();

        if (statusCode.HasValue)
        {
            this.logger.LogError("Garmin upload failed with HTTP {StatusCode} for user {UserId}.", (int)statusCode.Value, userId);
            return UploadResult.Failed($"Garmin upload failed: {ex.Message}", statusCode.Value);
        }

        this.logger.LogError(ex, "Garmin upload failed for user {UserId}.", userId);
        return UploadResult.Failed($"Garmin upload failed: {ex.Message}");
    }
}
