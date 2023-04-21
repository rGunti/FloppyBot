using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.Base.Exceptions;

public static class HttpStatusCodeExceptionExtensions
{
    public static HttpStatusCodeException NotImplemented(this ControllerBase controller)
    {
        return new RouteNotImplementedException(
            $"Route \"{controller.GetRequestIdentifier()}\" is not implemented"
        );
    }

    public static HttpStatusCodeException Obsolete(this ControllerBase controller)
    {
        return new BadRequestException(
            $"Route \"{controller.GetRequestIdentifier()}\" is marked obsolete and will not be implemented"
        );
    }

    public static HttpStatusCodeException UnsupportedFeature(
        this ControllerBase controller,
        string message
    )
    {
        return new UnsupportedV1OperationException(message, controller.GetRequestIdentifier());
    }

    private static string GetRequestIdentifier(this ControllerBase controllerBase)
    {
        return $"{controllerBase.Request.Method} {controllerBase.Request.Path}";
    }
}
