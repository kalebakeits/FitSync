using FitSync.Database.Models;
using FitSync.Uploader.Features.GarminUpload.DTOs;

namespace FitSync.Uploader.Features.GarminUpload;

public interface IGarminUploader
{
    Task<UploadResult> UploadActivityAsync(
        byte[] fitFileData,
        User user,
        CancellationToken cancellationToken = default
    );
}
