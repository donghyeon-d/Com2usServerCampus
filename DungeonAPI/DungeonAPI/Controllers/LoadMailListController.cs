using System;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.Services;
using DungeonAPI.RequestResponse;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoadMailListController : ControllerBase
{
    readonly ILogger<LoadMailListController> _logger;
    readonly IMailDb _mailDb;

    public LoadMailListController(ILogger<LoadMailListController> logger,
    IMailDb mail)
	{
        _logger = logger;
        _mailDb = mail;
    }

    [HttpPost]
    public async Task<LoadMailListRes> LoadMails(LoadMailListReq request)
    {
        LoadMailListRes response = new LoadMailListRes();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        // mail table에서 받아오기
        var (LoadMailErrorCode, mails) = await _mailDb.LoadMailListAtPage(playerId, request.ListNumber);
        if (LoadMailErrorCode != ErrorCode.None)
        {
            response.Result = LoadMailErrorCode;
            return response;
        }

        response.Mails = mails;
        return response;
    }
}

