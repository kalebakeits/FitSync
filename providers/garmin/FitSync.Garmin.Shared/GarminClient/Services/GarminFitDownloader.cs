namespace FitSync.Garmin.Shared.GarminClient.Services;

using System.IO.Compression;
using System.Net;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.Configuration;
using FitSync.Shared.Features.RateLimiting;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GarminFitDownloader(
    IGarminHttpSender httpSender,
    IRateLimiter rateLimiter,
    IOptions<GarminFetcherOptions> options,
    ILogger<GarminFitDownloader> logger
) : IGarminFitDownloader
{
    private const string DownloadPath = "/download-service/files/activity";
    private const string UserAgent = "GCM-iOS-5.7.2.1";
    private const string Origin = "https://sso.garmin.com";

    private readonly IGarminHttpSender httpSender = httpSender;
    private readonly IRateLimiter rateLimiter = rateLimiter;
    private readonly IOptions<GarminFetcherOptions> options = options;
    private readonly ILogger<GarminFitDownloader> logger = logger;

    public async Task<byte[]> DownloadFitFileAsync(
        Integration integration,
        long activityId,
        CancellationToken cancellationToken = default
    )
    {
        if (
            await this.rateLimiter.RateLimitedReachedAsync(
                ServiceType.GarminFetcher,
                this.options.Value.GarminApiRateLimits,
                cancellationToken
            )
        )
        {
            this.logger.LogWarning(
                "Garmin rate limit reached, skipping FIT download for activity {ActivityId}.",
                activityId
            );
            return [];
        }

        IFlurlResponse response = await this.httpSender.GetAsync(
            integration,
            accessToken =>
                $"{this.options.Value.BaseUrl}{DownloadPath}/{activityId}"
                    .WithOAuthBearerToken(accessToken)
                    .WithHeader("User-Agent", UserAgent)
                    .WithHeader("origin", Origin)
                    .WithHeader("NK", "NT")
                    .WithHeader("Accept", "*/*"),
            cancellationToken
        );

        // Garmin answers 404, and 500 for manually created activities, when an
        // activity has no associated FIT file.
        if (
            response.StatusCode
            is (int)HttpStatusCode.NotFound
                or (int)HttpStatusCode.InternalServerError
        )
        {
            throw new InvalidOperationException(
                $"Garmin has no FIT file for activity {activityId} ({response.StatusCode})."
            );
        }

        if (response.StatusCode >= 400)
        {
            string errorBody = await response.GetStringAsync();
            throw new InvalidOperationException(
                $"Garmin FIT download failed for activity {activityId} ({response.StatusCode}): {errorBody}"
            );
        }

        // This endpoint wraps the FIT in a ZIP named "{activityId}_ACTIVITY.fit"
        // rather than returning raw FIT bytes.
        byte[] archiveBytes = await response.GetBytesAsync();
        using MemoryStream archiveStream = new(archiveBytes);
        using ZipArchive archive = new(archiveStream, ZipArchiveMode.Read);

        string expectedEntryName = $"{activityId}_ACTIVITY.fit";
        ZipArchiveEntry? fitEntry =
            archive.Entries.FirstOrDefault(
                entry => entry.Name.Equals(expectedEntryName, StringComparison.OrdinalIgnoreCase)
            )
            ?? archive.Entries.FirstOrDefault(
                entry => entry.Name.EndsWith(".fit", StringComparison.OrdinalIgnoreCase)
            );

        if (fitEntry is null)
        {
            throw new InvalidOperationException(
                $"Garmin archive for activity {activityId} contained no FIT file."
            );
        }

        using Stream fitStream = fitEntry.Open();
        using MemoryStream fitBuffer = new();
        await fitStream.CopyToAsync(fitBuffer, cancellationToken);

        this.logger.LogInformation(
            "Downloaded {Length} FIT bytes for Garmin activity {ActivityId}.",
            fitBuffer.Length,
            activityId
        );

        return fitBuffer.ToArray();
    }
}
