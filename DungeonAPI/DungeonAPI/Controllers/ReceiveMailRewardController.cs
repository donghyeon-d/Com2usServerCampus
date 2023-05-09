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
    readonly IItemDb _itemDb;

    public ReceiveMailRewardController(ILogger<ReceiveMailRewardController> logger,
        IMailDb mailDb, IMailRewardDb mailRewardDb, IItemDb itemDb)
	{
        _logger = logger;
        _mailDb = mailDb;
        _mailRewardDb = mailRewardDb;
        _itemDb = itemDb;
	}

    [HttpPost]
    public async Task<ReceiveMailRewardRes> ReceiveMailReward(ReceiveMailRewardReq request)
    {
        var playerIdValue = HttpContext.Items["PlayerId"];
        Int32 playerId = int.Parse(playerIdValue.ToString());

        ReceiveMailRewardRes response = new ReceiveMailRewardRes();

        var MarkAsReceivedErrorCode = await _mailDb.MarkAsReceivedReward(request.MailId);
        if (MarkAsReceivedErrorCode != ErrorCode.None)
        {
            response.Result = MarkAsReceivedErrorCode;
            return response;
        }
        
        var (ReceiveMailRewardErrorCode, receivedRewards) = await _mailRewardDb.ReceiveMailRewards(playerId, request.MailId);

        response.MailRewards = receivedRewards;
        response.Result = ReceiveMailRewardErrorCode;
        
        return response;
    }
}

