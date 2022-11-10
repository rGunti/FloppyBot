using Microsoft.AspNetCore.Http;

namespace FloppyBot.WebApi.Base.Exceptions;

public class UnauthorizedException : HttpStatusCodeException
{
    public UnauthorizedException(string message) : base(StatusCodes.Status401Unauthorized, message)
    {
    }
}
