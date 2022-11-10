using Microsoft.AspNetCore.Http;

namespace FloppyBot.WebApi.Base.Exceptions;

public class RouteNotImplementedException : HttpStatusCodeException
{
    public RouteNotImplementedException() : base(StatusCodes.Status501NotImplemented)
    {
    }

    public RouteNotImplementedException(string message) : base(StatusCodes.Status501NotImplemented, message)
    {
    }
}
