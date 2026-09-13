namespace FitSync.Api.Exceptions;

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message)
        : base(403, message) { }

    public ForbiddenException(string message, Exception innerException)
        : base(403, message, innerException) { }
}
