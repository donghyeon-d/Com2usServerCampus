﻿using System;
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
    readonly IGameDb _gameDb;

    public ReadMailListController(ILogger<ReadMailListController> logger,
    IGameDb gameDb)
	{
        _logger = logger;
        _gameDb = gameDb;
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


        var (readMailErrorCode, mails) = await _gameDb.ReadMailListAtPage(playerId, request.ListNumber);
        if (readMailErrorCode != ErrorCode.None)
        {
            response.Result = readMailErrorCode;
            return response;
        }

        response.Mails = mails;
        return response;
    }
}

