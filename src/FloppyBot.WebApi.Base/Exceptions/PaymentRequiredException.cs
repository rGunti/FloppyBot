using Microsoft.AspNetCore.Http;

namespace FloppyBot.WebApi.Base.Exceptions;

public class PaymentRequiredException : HttpStatusCodeException
{
    public PaymentRequiredException(string message)
        : base(StatusCodes.Status402PaymentRequired, message) { }
}
