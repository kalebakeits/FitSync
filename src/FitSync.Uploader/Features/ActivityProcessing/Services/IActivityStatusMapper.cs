using System.Net;
using FitSync.Database.Enums;

namespace FitSync.Uploader.Features.ActivityProcessing.Services;

public interface IActivityStatusMapper
{
    ActivityStatus MapHttpStatusToActivityStatus(HttpStatusCode? statusCode);
}
