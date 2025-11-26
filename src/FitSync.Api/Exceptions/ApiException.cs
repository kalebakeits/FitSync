namespace FitSync.Api.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message)
        : base(message)
    {
        this.StatusCode = statusCode;
    }

    public ApiException(int statusCode, string message, Exception innerException)
        : base(message, innerException)
    {
        this.StatusCode = statusCode;
    }
}
