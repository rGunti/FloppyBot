using FloppyBot.WebApi.Base.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers;

[ApiController]
[Route("v1/test")]
public class V1TestController : ControllerBase
{
    [HttpGet]
    public string HelloWorld()
    {
        throw this.NotImplemented();
    }

    [HttpGet("a")]
    public string ExceptionTest()
    {
        throw new Exception("This is a test exception!");
    }
}
