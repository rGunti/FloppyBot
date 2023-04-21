using FloppyBot.WebApi.Base.Dtos;
using FloppyBot.WebApi.Base.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace FloppyBot.WebApi.Base.ExceptionHandler;

// ReSharper disable once ClassNeverInstantiated.Global
public class GlobalExceptionHandler : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

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
                _logger.LogWarning(context.Exception, "Unhandled exception occurred");
                context.Result = new ErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    $"{context.Exception.Message}",
                    context.Exception.GetType().ToString()
                ).ToJsonResult();
            }
        }
    }
}
