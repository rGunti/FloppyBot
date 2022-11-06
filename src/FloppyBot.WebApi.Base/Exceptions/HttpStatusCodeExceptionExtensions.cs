using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.Base.Exceptions;

public static class HttpStatusCodeExceptionExtensions
{
    public static HttpStatusCodeException NotImplemented(this ControllerBase controller)
    {
        var url = controller.Request.GetDisplayUrl();
        return new RouteNotImplementedException($"Route \"{url}\" is not implemented");
    }
}
