using FitSync.Database.Models;
using FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;

namespace FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;

public interface IUploadResultHandler
{
    Task HandleUploadResultAsync(
        Activity activity,
        ActivityUploadStatus uploadStatus,
        UploadResult result,
        int maxRetries
    );
}
