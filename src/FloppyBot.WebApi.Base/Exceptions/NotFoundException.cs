using Microsoft.AspNetCore.Http;

namespace FloppyBot.WebApi.Base.Exceptions;

public class NotFoundException : HttpStatusCodeException
{
    public NotFoundException() : base(StatusCodes.Status404NotFound)
    {
    }

    public NotFoundException(string message) : base(StatusCodes.Status404NotFound, message)
    {
    }
}
