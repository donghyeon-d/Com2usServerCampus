using System;
using DungeonAPI.RequestResponse;
using DungeonAPI.ModelDB;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReadMailContentController : ControllerBase
{
    readonly ILogger<ReadMailContentController> _logger;
    readonly IGameDb _gameDb;

    public ReadMailContentController(ILogger<ReadMailContentController> logger,
    IGameDb gameDb)
	{
        _logger = logger;
        _gameDb = gameDb;
	}

    [HttpPost]
    public async Task<ReadMailContentRes> ProcessRequest(ReadMailContentReq request)
    {
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        ReadMailContentRes response = await LoadMailContent(request, player.Id);

        _logger.ZLogInformationWithPayload(new { PlayerId = player.Id, RequestMailId = request.MailId }, response.Result.ToString());

        return response;
    }

    async Task <ReadMailContentRes>LoadMailContent(ReadMailContentReq request, Int32 playerId)
    {
        ReadMailContentRes response = new();


        var (readMailContentErrorCode, mailContent) = await _gameDb.ReadMailContent(playerId, request.MailId);
        if (readMailContentErrorCode != ErrorCode.None)
        {
            response.Result = readMailContentErrorCode;
            return response;
        }

        var OpenMailErrorCode = await _gameDb.MarkAsOpenMail(request.MailId, playerId);
        if (OpenMailErrorCode != ErrorCode.None)
        {
            response.Result = OpenMailErrorCode;
            return response;
        }

        response.MailContent = mailContent;
        
        return response;
    }
}

