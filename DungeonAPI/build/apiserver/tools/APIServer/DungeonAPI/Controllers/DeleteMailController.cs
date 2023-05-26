using DungeonAPI.ModelDB;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DeleteMailController : ControllerBase
{
    readonly ILogger<DeleteMailController> _logger;
    readonly IGameDb _gameDb;

    public DeleteMailController(ILogger<DeleteMailController> logger,
        IGameDb gameDb)
	{
        _logger = logger;
        _gameDb = gameDb;
	}

    [HttpPost]
    public async Task<DeleteMailRes> MarkAsDeleteMail(DeleteMailReq request)
    {
        DeleteMailRes response = new DeleteMailRes();

        var player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        var MarkAsDeleteMailErrorCode = await _gameDb.MarkAsDeleteMail(request.MailId, player.Id);
        if (MarkAsDeleteMailErrorCode != ErrorCode.None)
        {
            response.Result = MarkAsDeleteMailErrorCode;
            _logger.ZLogInformationWithPayload(new { PlayerId = player.Id, MailId = request.MailId }, response.Result.ToString());
            return response;
        }

        _logger.ZLogInformationWithPayload(new { PlayerId = player.Id, MailId = request.MailId }, response.Result.ToString());
        return response;
    }
}

