using FloppyBot.Base.Extensions;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.DataImport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/import")]
// [Authorize(Permissions.READ_BOT)]
public class V1DataImportController : ControllerBase
{
    private readonly V1DataImportService _importService;

    public V1DataImportController(V1DataImportService importService)
    {
        _importService = importService;
    }

    [HttpPost]
    public IActionResult ImportData(IFormFile? file, [FromQuery] bool simulate = true)
    {
        if (file == null)
        {
            throw new BadRequestException("No file provided");
        }

        if (file.Length > 5.MegaBytes())
        {
            throw new PayloadTooLargeException("File is larger than allowed");
        }

        if (!_importService.ProcessFile(file.OpenReadStream(), User.TryGetUserId(), simulate))
        {
            throw new HttpStatusCodeException(
                StatusCodes.Status500InternalServerError,
                "Failed to import file"
            );
        }

        return NoContent();
    }
}
