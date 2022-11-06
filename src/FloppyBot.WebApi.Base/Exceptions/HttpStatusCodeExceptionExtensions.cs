using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.Base.Exceptions;

public static class HttpStatusCodeExceptionExtensions
{
    public static HttpStatusCodeException NotImplemented(this ControllerBase controller)
    {
        return new RouteNotImplementedException(
            $"Route \"{controller.Request.Method} {controller.Request.Path}\" is not implemented");
    }
}
