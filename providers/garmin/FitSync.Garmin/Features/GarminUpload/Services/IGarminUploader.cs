namespace FitSync.Garmin.Features.GarminUpload;

using FitSync.Database.Models;
using FitSync.Garmin.Shared.GarminClient.DTOs;

public interface IGarminUploader
{
    Task<UploadResult> UploadActivityAsync(
        byte[] fitFileData,
        User user,
        CancellationToken cancellationToken = default
    );
}
