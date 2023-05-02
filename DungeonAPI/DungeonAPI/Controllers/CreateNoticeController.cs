using System;
using DungeonAPI.Configs;
using DungeonAPI.MessageBody;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CreateNoticeController
{
    readonly ILogger<CreateNoticeController> _logger;
    readonly INotice _notice;
    readonly IOptions<AdminConfig> _admin;

    public CreateNoticeController(ILogger<CreateNoticeController> logger, INotice notice, IOptions<AdminConfig> admin)
	{
        _logger = logger;
        _notice = notice;
        _admin = admin;
	}

    [HttpPost]
    public async Task<CreateNoticeRes> CreateNotice(CreateNoticeReq request)
    {
        CreateNoticeRes response = new CreateNoticeRes();
        if (request.Email != _admin.Value.Email || request.Password != _admin.Value.Password)
        {
            response.ErrorCode = ErrorCode.NoticeAuthFail;
            return response;
        }

        ErrorCode duplicatedTitleErrorCode = await CheckDuplicateTitle(request.Title);
        if (duplicatedTitleErrorCode != ErrorCode.None)
        {
            response.ErrorCode = duplicatedTitleErrorCode;
            return response;
        }

        ErrorCode createNoticeErrorCode = await _notice.CreateNotification(request.Title, request.Content, request.Date);
        if (createNoticeErrorCode != ErrorCode.None)
        {
            response.ErrorCode = createNoticeErrorCode;
            return response;
        }

        return response;
    }

    async Task<ErrorCode> CheckDuplicateTitle(String title)
    {
        var (LoadNoticeErrorCode, notices) = await _notice.LoadAllNotification();
        if (LoadNoticeErrorCode != ErrorCode.None)
        {
            return LoadNoticeErrorCode;
        }

        foreach (Notification notice in notices)
        {
            if (notice.Title == title)
            {
                return ErrorCode.NoticeDuplicatedTitile;
            }
        }
        return ErrorCode.None;
    }
}

