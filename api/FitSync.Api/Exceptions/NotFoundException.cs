namespace FitSync.Api.Exceptions;

public class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(404, message) { }

    public NotFoundException(string message, Exception innerException)
        : base(404, message, innerException) { }
}
