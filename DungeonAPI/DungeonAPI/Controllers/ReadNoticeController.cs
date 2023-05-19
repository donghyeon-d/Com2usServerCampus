using System;
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
    public async Task<ReadNoticeRes> ProcessRequest(ReadNoticeReq request)
    {
        ReadNoticeRes respones = await LoadNotice(request);

        _logger.ZLogInformationWithPayload(new { Email = request.Email }, respones.Result.ToString());

        return respones;

    }

    async Task<ReadNoticeRes> LoadNotice(ReadNoticeReq request)
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

