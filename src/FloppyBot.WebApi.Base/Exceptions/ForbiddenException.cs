using Microsoft.AspNetCore.Http;

namespace FloppyBot.WebApi.Base.Exceptions;

public class ForbiddenException : HttpStatusCodeException
{
    public ForbiddenException(string message) : base(StatusCodes.Status403Forbidden, message)
    {
    }
}
