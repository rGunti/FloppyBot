using Microsoft.AspNetCore.Http;

namespace FloppyBot.WebApi.Base.Exceptions;

public class ConflictException : HttpStatusCodeException
{
    public ConflictException(string message) : base(StatusCodes.Status409Conflict, message)
    {
    }
}
