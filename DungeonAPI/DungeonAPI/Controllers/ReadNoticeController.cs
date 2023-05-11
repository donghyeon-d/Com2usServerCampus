using System;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReadNoticeController : ControllerBase
{
    readonly ILogger<ReadNoticeController> _logger;
    readonly INoticeDb _notice;

    public ReadNoticeController(ILogger<ReadNoticeController> logger, INoticeDb notice)
    {
        _logger = logger;
        _notice = notice;
    }

    [HttpPost]
    public async Task<ReadNoticeRes> LoadNotice()
    {
        ReadNoticeRes respones = new ();

        var (LoadNoticeErrorCode, notices) = await _notice.ReadNotificationList();
        if (LoadNoticeErrorCode != ErrorCode.None || notices is null)
        {
            respones.ErrorCode = LoadNoticeErrorCode;
            return respones;
        }

        respones.Notices = notices;
        return respones;
    }
}

