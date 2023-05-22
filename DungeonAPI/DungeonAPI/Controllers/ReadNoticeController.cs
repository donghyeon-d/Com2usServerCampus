﻿using System;
using DungeonAPI.ModelDB;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReadNoticeController : ControllerBase
{
    readonly ILogger<ReadNoticeController> _logger;
    readonly INoticeMemoryDb _notice;

    public ReadNoticeController(ILogger<ReadNoticeController> logger, INoticeMemoryDb notice)
    {
        _logger = logger;
        _notice = notice;
    }

    [HttpPost]
    public async Task<ReadNoticeRes> ProcessRequest()
    {
        ReadNoticeRes respones = await LoadNotice();

        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        _logger.ZLogInformationWithPayload(new { PlayerId = player.Id }, respones.Result.ToString());

        return respones;

    }

    async Task<ReadNoticeRes> LoadNotice()
    {
        ReadNoticeRes respones = new();

        var (LoadNoticeErrorCode, notices) = await _notice.ReadNotificationList();
        if (LoadNoticeErrorCode != ErrorCode.None || notices is null)
        {
            respones.Result = LoadNoticeErrorCode;
            return respones;
        }

        respones.Notices = notices;
        return respones;
    }
}

