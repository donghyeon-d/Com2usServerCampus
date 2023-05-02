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
    readonly INotice _notice;

    public LoadNoticeController(ILogger<LoadNoticeController> logger, INotice notice)
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
        respones.notices = notices;
        return respones;
    }
}

