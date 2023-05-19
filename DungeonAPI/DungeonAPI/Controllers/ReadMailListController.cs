using System;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.Services;
using DungeonAPI.RequestResponse;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReadMailListController : ControllerBase
{
    readonly ILogger<ReadMailListController> _logger;
    readonly IMailDb _mailDb;

    public ReadMailListController(ILogger<ReadMailListController> logger,
    IMailDb mail)
	{
        _logger = logger;
        _mailDb = mail;
    }

    [HttpPost] 
    public async Task<ReadMailListRes> ProcessRequest(ReadMailListReq request)
    {
        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        ReadMailListRes response = await LoadMails(request, playerId);

        _logger.ZLogInformationWithPayload(new { Email = request.Email, RequestListNumber = request.ListNumber },
            response.Result.ToString());

        return response;
    }

    async Task<ReadMailListRes> LoadMails(ReadMailListReq request, Int32 playerId)
    {
        ReadMailListRes response = new();


        var (readMailErrorCode, mails) = await _mailDb.ReadMailListAtPage(playerId, request.ListNumber);
        if (readMailErrorCode != ErrorCode.None)
        {
            response.Result = readMailErrorCode;
            return response;
        }

        response.Mails = mails;
        return response;
    }
}

