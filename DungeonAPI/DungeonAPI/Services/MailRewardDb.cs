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
    readonly IItemDb _itemDb;

	public MailRewardDb(ILogger<MailRewardDb> logger, IOptions<DbConfig> dbConfig, IItemDb itemDb)
        : base(logger, dbConfig)
    {
        _logger = logger;
        _itemDb = itemDb;
    }

    public async Task<ErrorCode> CreateMailRewards(Int32 mailId, List<MailReward> rewards)
    {
        // TODO : log
        try
        {
            var cols = new[] { "MailId", "BaseItemCode", "ItemCount", "IsReceived" };
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
                reward.ItemCount,
                false
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

    public async Task<Tuple<ErrorCode, List<MailReward>?>> ReceiveMailRewards(Int32 playerId, Int32 mailId)
    {
        List<MailReward>? mailRewardresult = new List<MailReward>();

        var (loadMailRewardErrorCode, mailsRewards) = await LoadMailRewards(mailId);
        if (loadMailRewardErrorCode != ErrorCode.None)
        {
            return new Tuple<ErrorCode, List<MailReward>?>(loadMailRewardErrorCode, mailRewardresult);
        }
        else if (mailsRewards == null) 
        {
            return new Tuple<ErrorCode, List<MailReward>?>(ErrorCode.MailRewardNotFound, mailRewardresult);
        }

        var canReceiveRewards = mailsRewards.FindAll(reward => reward.IsReceived == false);
        if (canReceiveRewards == null)
        {
            return new Tuple<ErrorCode, List<MailReward>?>(ErrorCode.MailRewardAlreadyReceived, mailRewardresult);
        }

        foreach (var mailReward in canReceiveRewards)
        {
            Item rewardItem = MakeRewardItem(mailReward, playerId);
            var (AcquireItemdErrorCode, newItemId) = await _itemDb.AcquiredItem(playerId, rewardItem);
            if (AcquireItemdErrorCode == ErrorCode.None)
            {
                ErrorCode MarkAsReceivedRewardErrorCode = await MarkAsReceivedReward(mailReward.MailRewardId);
                if (MarkAsReceivedRewardErrorCode != ErrorCode.None)
                {
                    await _itemDb.DeleteItemByItemId(newItemId);
                }
            }
            mailRewardresult.Add(mailReward);
        }

        if (canReceiveRewards.Count != mailRewardresult.Count)
        {
            return new Tuple<ErrorCode, List<MailReward>?>(ErrorCode.ReceiveRewardButSomeLoss, mailRewardresult);
        }

        return new Tuple<ErrorCode, List<MailReward>?>(ErrorCode.None, mailRewardresult);
    }

    async Task<ErrorCode> MarkAsReceivedReward(Int32 rewardId)
    {
        var count = await _queryFactory.Query("MailReward")
                                    .Where("MailRewardId", rewardId)
                                    .UpdateAsync(true);
        if (count != 0)
        {
            return ErrorCode.MarkAsReceivedRewardUpdateFail;
        }
        return ErrorCode.None;
    }

    Item MakeRewardItem(MailReward mailReward, Int32 playerId)
    {
        MasterData.BaseItem? baseItem = MasterDataDb.s_baseItem.Find(item => item.Code == mailReward.BaseItemCode);
        if (baseItem == null)
        {
            return null;
        }

        Item item= new Item();
        item.PlayerId = playerId;
        item.ItemMasterDataCode = mailReward.BaseItemCode;
        item.ItemCount = mailReward.ItemCount;
        item.Attack = baseItem.Attack;
        item.Defence = baseItem.Defence;
        item.Magic = baseItem.Magic;
        item.EnhanceLevel = 0;
        item.RemainingEnhanceCount = baseItem.EnhanceMaxCount;
        item.IsDestructed = 0;
        return item;
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

