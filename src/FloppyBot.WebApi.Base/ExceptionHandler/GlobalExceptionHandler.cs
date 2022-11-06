using FloppyBot.WebApi.Base.Dtos;
using FloppyBot.WebApi.Base.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FloppyBot.WebApi.Base.ExceptionHandler;

public class GlobalExceptionHandler : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception != null && !context.ExceptionHandled)
        {
            context.ExceptionHandled = true;
            if (context.Exception is HttpStatusCodeException ex)
            {
                context.Result = ex.GetErrorResponse().ToJsonResult();
            }
            else
            {
                context.Result = new ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        $"{context.Exception.Message}",
                        context.Exception.GetType().ToString())
                    .ToJsonResult();
            }
        }
    }
}
