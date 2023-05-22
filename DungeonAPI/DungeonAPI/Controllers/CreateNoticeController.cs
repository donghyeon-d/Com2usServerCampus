// Notice DB에 정보를 넣기 위한 임시 API
// 실제 서버 구현할때는 관리 툴에서 작업해줘야 함
// 클라이언트가 접근할 수 있는 서버이기 때문에 보안상 매우 좋지 않음

using System;
using DungeonAPI.Configs;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CreateNoticeController : ControllerBase
{
    readonly INoticeMemoryDb _notice;
    readonly IOptions<AdminConfig> _admin;

    public CreateNoticeController(INoticeMemoryDb notice, IOptions<AdminConfig> admin)
	{
        _notice = notice;
        _admin = admin;
	}

    [HttpPost]
    public async Task<CreateNoticeRes> CreateNotice(CreateNoticeReq request)
    {
        CreateNoticeRes response = new CreateNoticeRes();
        if (request.Email != _admin.Value.Email || request.Password != _admin.Value.Password)
        {
            response.Result = ErrorCode.NoticeAuthFail;
            return response;
        }

        ErrorCode duplicatedTitleErrorCode = await CheckDuplicateTitle(request.Title);
        if (duplicatedTitleErrorCode != ErrorCode.None)
        {
            response.Result = duplicatedTitleErrorCode;
            return response;
        }

        ErrorCode createNoticeErrorCode = await _notice.CreateNotification(request.Title, request.Content, request.Date);
        if (createNoticeErrorCode != ErrorCode.None)
        {
            response.Result = createNoticeErrorCode;
            return response;
        }

        return response;
    }

    async Task<ErrorCode> CheckDuplicateTitle(String title)
    {
        var (LoadNoticeErrorCode, notices) = await _notice.ReadNotificationList();
        if (LoadNoticeErrorCode != ErrorCode.None || notices is null)
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

