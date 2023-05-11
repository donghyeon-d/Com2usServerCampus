using System;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.Services;
using DungeonAPI.RequestResponse;

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
    public async Task<ReadMailListRes> LoadMails(ReadMailListReq request)
    {
        ReadMailListRes response = new ();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

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

