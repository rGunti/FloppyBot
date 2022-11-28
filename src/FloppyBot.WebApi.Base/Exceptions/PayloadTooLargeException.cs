using Microsoft.AspNetCore.Http;

namespace FloppyBot.WebApi.Base.Exceptions;

public class PayloadTooLargeException : HttpStatusCodeException
{
    public PayloadTooLargeException(string message) : base(StatusCodes.Status413PayloadTooLarge, message)
    {
    }
}
