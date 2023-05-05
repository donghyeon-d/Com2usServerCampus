using System;
using DungeonAPI.MessageBody;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReceiveMailRewardController : ControllerBase
{
    readonly ILogger<ReceiveMailRewardController> _logger;
    readonly IMailDb _mailDb;
    readonly IMailRewardDb _mailRewardDb;

    public ReceiveMailRewardController(ILogger<ReceiveMailRewardController> logger,
        IMailDb mailDb, IMailRewardDb mailRewardDb)
	{
        _logger = logger;
        _mailDb = mailDb;
        _mailRewardDb = mailRewardDb;
	}

    [HttpPost]
    public async Task<ReceiveMailRewardRes> ReceiveMailReward(ReceiveMailRewardReq request)
    {
        ReceiveMailRewardRes response = new ReceiveMailRewardRes();

        var MarkAsReceivedErrorCode = await _mailDb.MarkAsReceivedReward(request.MailId);
        if (MarkAsReceivedErrorCode != ErrorCode.None)
        {
            response.Result = MarkAsReceivedErrorCode;
            return response;
        }

        var (LoadMailRewardsErrorCode, Rewards) = await _mailRewardDb.LoadMailRewards(request.MailId);
        if (LoadMailRewardsErrorCode != ErrorCode.None)
        {
            response.Result = LoadMailRewardsErrorCode;
            return response;
        }

        response.MailRewards = Rewards;
        return response;
    }
}

