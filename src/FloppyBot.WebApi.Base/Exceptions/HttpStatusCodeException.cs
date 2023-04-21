using FloppyBot.WebApi.Base.Dtos;

namespace FloppyBot.WebApi.Base.Exceptions;

public class HttpStatusCodeException : Exception
{
    public HttpStatusCodeException(int statusCode)
        : this(statusCode, $"Returning HTTP {statusCode}") { }

    public HttpStatusCodeException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }

    public virtual ErrorResponse GetErrorResponse()
    {
        return new ErrorResponse(StatusCode, Message);
    }
}
