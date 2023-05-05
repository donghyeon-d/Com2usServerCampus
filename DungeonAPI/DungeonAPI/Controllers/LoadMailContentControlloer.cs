using System;
using DungeonAPI.MessageBody;
using DungeonAPI.ModelDB;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoadMailContentControlloer : ControllerBase
{
    readonly ILogger<LoadMailContentControlloer> _logger;
    readonly IMailDb _mail;
    readonly IMailContentDb _mailContent;
    readonly IMailRewardDb _mailReward;
    readonly IHttpContextAccessor _httpContextAccessor;

    public LoadMailContentControlloer(ILogger<LoadMailContentControlloer> logger,
    IMailDb mail, IMailContentDb mailContent, IMailRewardDb mailReward,
    IHttpContextAccessor httpContextAccessor)
	{
        _logger = logger;
        _mail = mail;
        _mailContent = mailContent;
        _mailReward = mailReward;
        _httpContextAccessor = httpContextAccessor;
	}

    // 컨텐츠, 보상 주고 열었음으로 표. 
    [HttpPost]
    public async Task<LoadMailContentRes> LoadMailContent(LoadMailContentReq request)
    {
        LoadMailContentRes response = new LoadMailContentRes();

        var (LoadMailContentErrorCode, mailContent) = await _mailContent.LoadMailContent(request.MailId);
        if (LoadMailContentErrorCode != ErrorCode.None)
        {
            response.Result = LoadMailContentErrorCode;
            return response;
        }

        var (LoadMailRewardErrorCode, mailRewards) = await _mailReward.LoadMailRewards(request.MailId);
        if (LoadMailRewardErrorCode != ErrorCode.None)
        {
            response.Result = LoadMailRewardErrorCode;
            return response;
        }

        var OpenMailErrorCode = await _mail.OpenMail(request.MailId);
        if (OpenMailErrorCode != ErrorCode.None)
        {
            response.Result = OpenMailErrorCode;
            return response;
        }

        response.MailContent = mailContent;
        response.MailRewards = mailRewards;
        return response;
    }
}

