namespace FitSync.Api.Exceptions;

public class BadRequestException : ApiException
{
    public BadRequestException(string message)
        : base(400, message) { }

    public BadRequestException(string message, Exception innerException)
        : base(400, message, innerException) { }
}
