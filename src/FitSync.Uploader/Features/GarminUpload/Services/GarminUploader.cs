namespace FitSync.Uploader.Features.GarminUpload.Services;

using System.Net;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Uploader.Features.GarminUpload.DTOs;
using Garmin.Connect;
using Garmin.Connect.Auth;
using Garmin.Connect.Auth.External;
using Garmin.Connect.Exceptions;
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
        var garminCred = await fitSyncDbContext.UserCredentials.FirstOrDefaultAsync(
            c => c.UserId == user.Id && c.ServiceType == ServiceTypes.Garmin,
            cancellationToken
        );

        if (garminCred == null)
        {
            this.logger.LogError("No Garmin credentials found for user {UserId}", user.Id);
            return UploadResult.Failed("No Garmin credentials found");
        }

        (string username, string password) = garminCred.Decrypt(this.encryptionService);
        BasicAuthParameters authParameters = new(username, password);

        Garmin.Connect.GarminConnectClient client =
            new(new GarminConnectContext(new HttpClient(), authParameters));

        string tempFile = Path.Combine(Path.GetTempPath(), $"fit_{Guid.NewGuid()}.fit");
        await File.WriteAllBytesAsync(tempFile, fitFileData, cancellationToken);

        try
        {
            await client.UploadFile(tempFile, cancellationToken);

            this.logger.LogInformation("Upload successful for user {Username}", user.Username);
            return UploadResult.Succeeded();
        }
        catch (GarminConnectRequestException ex)
        {
            return HandleException(ex, ex.Status, user.Id);
        }
        catch (GarminConnectAuthenticationException ex)
        {
            return HandleException(ex, HttpStatusCode.Unauthorized, user.Id);
        }
        catch (HttpRequestException ex)
        {
            return HandleException(ex, ex.StatusCode, user.Id);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Unexpected error during Garmin upload for user {UserId}",
                user.Id
            );
            return UploadResult.Failed($"Unexpected error: {ex.Message}");
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private UploadResult HandleException(Exception ex, HttpStatusCode? statusCode, Guid userId)
    {
        if (statusCode.HasValue && ((int)statusCode >= 200) && ((int)statusCode <= 299))
        {
            return UploadResult.Succeeded();
        }
        if (statusCode.HasValue)
        {
            this.logger.LogError(
                "Garmin upload failed with HTTP {StatusCode} for user {UserId}",
                (int)statusCode.Value,
                userId
            );

            return UploadResult.Failed($"Garmin upload failed: {ex.Message}", statusCode.Value);
        }

        this.logger.LogError(ex, "Garmin upload failed for user {UserId}", userId);
        return UploadResult.Failed($"Garmin upload failed: {ex.Message}");
    }
}
