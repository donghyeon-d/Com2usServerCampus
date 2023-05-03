using System;
using DungeonAPI.MessageBody;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoadNoticeController
{
    readonly ILogger<LoadNoticeController> _logger;
    readonly INoticeDb _notice;

    public LoadNoticeController(ILogger<LoadNoticeController> logger, INoticeDb notice)
    {
        _logger = logger;
        _notice = notice;
    }

    [HttpPost]
    public async Task<LoadNoticeRes> LoadNotice(LoadNoticeReq request)
    {
        LoadNoticeRes respones = new LoadNoticeRes();

        var (LoadNoticeErrorCode, notices) = await _notice.LoadAllNotification();
        if (LoadNoticeErrorCode != ErrorCode.None)
        {
            respones.ErrorCode = LoadNoticeErrorCode;
            return respones;
        }
        respones.Notices = notices;
        return respones;
    }
}

