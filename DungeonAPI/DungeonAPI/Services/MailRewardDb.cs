using System;
using System.Collections.Generic;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;

namespace DungeonAPI.Services;

public class MailRewardDb : GameDb, IMailRewardDb
{
    readonly ILogger<MailRewardDb> _logger;

	public MailRewardDb(ILogger<MailRewardDb> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
    {
        _logger = logger;
    }

    public async Task<ErrorCode> CreateMailRewards(Int32 mailId, List<MailReward> rewards)
    {
        // TODO : log
        try
        {
            var cols = new[] { "MailId", "BaseItemCode", "ItemCount" };
            List<object[]> rewardItems = MakeReward(mailId, rewards);
            var count = await _queryFactory.Query("MailReward").InsertAsync(cols, rewardItems);
            if (count != rewardItems.Count)
            {
                return ErrorCode.MailRewardCreateFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.MailRewardCreateFailException;
        }
        finally
        {
            Dispose();
        }
    }

    List<object[]> MakeReward(Int32 mailId, List<MailReward> rewards)
    {
        List<object[]> result = new List<object[]>();
        
        foreach (var reward in rewards)
        {
            result.Add(new object[]
            {
                mailId,
                reward.BaseItemCode,
                reward.ItemCount
            });
        }
        return result;
    }

    public async Task<Tuple<ErrorCode, List<MailReward>>> LoadMailRewards(Int32 mailId)
    {
        try
        {
            var result = await _queryFactory.Query("MailReward")
                                           .Where("MailId", mailId)
                                           .GetAsync<MailReward>();
            if (result is null)
            {
                return new Tuple<ErrorCode, List<MailReward>>(ErrorCode.MailRewardLoadFail, null);
            }
            List<MailReward> mailRewards = result.ToList();
            return new Tuple<ErrorCode, List<MailReward>>(ErrorCode.None, mailRewards);
        }
        catch (Exception e)
        {
            // TODO: log
            return new Tuple<ErrorCode, List<MailReward>>(ErrorCode.MailRewardLoadFailException, null);
        }
        finally
        {
            Dispose();
        }
    }

    public async Task<ErrorCode> DeleteMailReward(Int32 mailId)
    {
        try
        {
            var mailReward = await _queryFactory.Query("MailReward")
                                           .Where("MailId", mailId)
                                           .DeleteAsync();
            if (mailReward != 1)
            {
                return ErrorCode.MailRewardDeleteFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            // TODO: log
            return ErrorCode.MailRewardDeleteFailException;
        }
        finally
        {
            Dispose();
        }
    }
}

