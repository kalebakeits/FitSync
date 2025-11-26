namespace FitSync.Api.Exceptions;

public class ConflictException : ApiException
{
    public ConflictException(string message)
        : base(409, message) { }

    public ConflictException(string message, Exception innerException)
        : base(409, message, innerException) { }
}
