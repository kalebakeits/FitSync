using System.Net;
using FitSync.Database.Enums;

namespace FitSync.Uploader.Features.ActivityProcessing.Services;

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
