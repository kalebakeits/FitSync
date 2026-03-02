using FitSync.Database.Models;
using FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;

namespace FitSync.Garmin.Uploader.Features.GarminUpload;

public interface IGarminUploader
{
    Task<UploadResult> UploadActivityAsync(
        byte[] fitFileData,
        User user,
        CancellationToken cancellationToken = default
    );
}
