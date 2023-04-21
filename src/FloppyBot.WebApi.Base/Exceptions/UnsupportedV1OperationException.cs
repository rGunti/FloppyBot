using FloppyBot.WebApi.Base.Dtos;

namespace FloppyBot.WebApi.Base.Exceptions;

/// <summary>
/// This exception will result in an error response that signals to the
/// Admin Console that this feature will not be available in V2 and that
/// there is no suitable compatibility implementation for it.
/// The Admin Console is instructed to show a blocking error to the user
/// and prevent them from interacting with this particular feature any
/// further.
/// </summary>
public class UnsupportedV1OperationException : HttpStatusCodeException
{
    public UnsupportedV1OperationException(string message, string? route = null)
        : base(590, message)
    {
        Route = route;
    }

    public string? Route { get; }

    public override ErrorResponse GetErrorResponse()
    {
        return new UnavailableFeatureResponse(StatusCode, Message, Route);
    }
}
