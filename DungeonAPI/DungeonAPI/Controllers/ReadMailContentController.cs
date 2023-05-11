using System;
using DungeonAPI.RequestResponse;
using DungeonAPI.ModelDB;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReadMailContentController : ControllerBase
{
    readonly ILogger<ReadMailContentController> _logger;
    readonly IMailDb _mailDb;

    public ReadMailContentController(ILogger<ReadMailContentController> logger,
    IMailDb mail)
	{
        _logger = logger;
        _mailDb = mail;
	}

    [HttpPost]
    public async Task<ReadMailContentRes> LoadMailContent(ReadMailContentReq request)
    {
        ReadMailContentRes response = new ();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        var (readMailContentErrorCode, mailContent) = await _mailDb.ReadMailContent(playerId, request.MailId);
        if (readMailContentErrorCode != ErrorCode.None)
        {
            response.Result = readMailContentErrorCode;
            return response;
        }

        var OpenMailErrorCode = await _mailDb.MarkAsOpenMail(request.MailId, playerId);
        if (OpenMailErrorCode != ErrorCode.None)
        {
            response.Result = OpenMailErrorCode;
            return response;
        }

        response.MailContent = mailContent;
        return response;
    }
}

