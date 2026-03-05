namespace FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;

using System.Net;
using FitSync.Database.Enums;

public class ActivityStatusMapper : IActivityStatusMapper
{
    public ActivityStatus MapHttpStatusToActivityStatus(HttpStatusCode? statusCode)
    {
        if (!statusCode.HasValue)
        {
            return ActivityStatus.Failed;
        }

        return statusCode.Value switch
        {
            HttpStatusCode.Conflict => ActivityStatus.Conflict,
            HttpStatusCode.ServiceUnavailable => ActivityStatus.ServiceUnavailable,
            HttpStatusCode.BadGateway => ActivityStatus.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout => ActivityStatus.ServiceUnavailable,
            >= HttpStatusCode.InternalServerError => ActivityStatus.ServiceUnavailable,
            _ => ActivityStatus.Failed
        };
    }
}
