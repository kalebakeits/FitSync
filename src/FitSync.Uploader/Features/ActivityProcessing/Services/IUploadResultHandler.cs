using FitSync.Database.Models;
using FitSync.Uploader.Features.GarminUpload.DTOs;

namespace FitSync.Uploader.Features.ActivityProcessing.Services;

public interface IUploadResultHandler
{
    Task HandleUploadResultAsync(Activity activity, UploadResult result, int maxRetries);
}
