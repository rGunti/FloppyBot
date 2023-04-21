using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.Base.Dtos;

public record ErrorResponse(int StatusCode, string Message, string? Source = null);

public static class ErrorResponseExceptions
{
    public static JsonResult ToJsonResult(this ErrorResponse response)
    {
        return new JsonResult(response) { StatusCode = response.StatusCode, };
    }
}
