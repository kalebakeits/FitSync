namespace FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;

using System.Net;
using FitSync.Database.Enums;

public interface IActivityStatusMapper
{
    ActivityStatus MapHttpStatusToActivityStatus(HttpStatusCode? statusCode);
}
