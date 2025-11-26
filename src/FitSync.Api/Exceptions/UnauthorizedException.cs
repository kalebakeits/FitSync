namespace FitSync.Api.Exceptions;

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message)
        : base(401, message) { }

    public UnauthorizedException(string message, Exception innerException)
        : base(401, message, innerException) { }
}
