using System;
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

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        var MarkAsDeleteMailErrorCode = await _gameDb.MarkAsDeleteMail(request.MailId, playerId);
        if (MarkAsDeleteMailErrorCode != ErrorCode.None)
        {
            response.Result = MarkAsDeleteMailErrorCode;
            _logger.ZLogInformationWithPayload(new { Email = request.Email, MailId = request.MailId }, response.Result.ToString());
            return response;
        }

        _logger.ZLogInformationWithPayload(new { Email = request.Email, MailId = request.MailId }, response.Result.ToString());
        return response;
    }
}

