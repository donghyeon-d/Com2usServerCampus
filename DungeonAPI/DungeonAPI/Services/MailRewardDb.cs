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

    public async Task<ErrorCode> CreateMailRewards(Int32 mailId, IEnumerable<MailReward> rewards)
    {
        // TODO : log
        try
        {
            var count = await _queryFactory.Query("MailReward").InsertAsync(rewards);
            if (count != 1)
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

    public async Task<Tuple<ErrorCode, List<MailReward>>> LoadMailReward(Int32 mailId)
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

