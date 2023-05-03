using System;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.Services;
using DungeonAPI.MessageBody;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoadMailController
{
    readonly ILogger<LoadMailController> _logger;
    readonly IMailDb _mail;
    readonly IMailRewardDb _mailReward;
    readonly IAuthUserDb _authUser;

    public LoadMailController(ILogger<LoadMailController> logger,
    IMailDb mail, IMailRewardDb mailReward, IAuthUserDb authUser)
	{
        _logger = logger;
        _mail = mail;
        _mailReward = mailReward;
        _authUser = authUser;
    }

    [HttpPost]
    public async Task<LoadMailRes> LoadMails(LoadMailReq request)
    {
        LoadMailRes response = new LoadMailRes();

        // player id redis에서 받아오기
        var (LoadAuthUserErrorCode, authUser) = await _authUser.LoadAuthUserByEmail(request.Email);
        if (LoadAuthUserErrorCode != ErrorCode.None)
        {
            response.Result = LoadAuthUserErrorCode;
            return response;
        }

        // mail table에서 받아오기
        var (LoadMailErrorCode, mails) = await _mail.LoadMailAt(authUser.PlayerId, request.ListNumber);
        if (LoadMailErrorCode != ErrorCode.None)
        {
            response.Result = LoadMailErrorCode;
            return response;
        }

        // mail reward table에서 받아오기
        Int32 mailId = mails[0].MailId;
        var (LoadMailRewardErrorCode, rewards) = await _mailReward.LoadMailReward(mailId);
        if (LoadMailRewardErrorCode != ErrorCode.None)
        {
            response.Result = LoadMailRewardErrorCode;
            return response;
        }

        response.Mails = CreateMailswithRewards(mails, rewards);
        return response;
    }

    List<MailWithReward> CreateMailswithRewards(List<Mail> mails, List<MailReward> rewards)
    {
        List<MailWithReward> mailWithRewards = new List<MailWithReward>();

        foreach (Mail mail in mails)
        {
            MailWithReward mailWithReward = new MailWithReward(mail);
            foreach (ModelDB.MailReward reward in rewards)
            {
                if (reward.MailId == mailWithReward.MailId)
                {
                    mailWithReward.Rewards.Add(reward);
                }
            }
            mailWithRewards.Add(mailWithReward);
        }

        return mailWithRewards;
    }
}

