﻿using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface IMailRewardService
{
    public Task<ErrorCode> CreateMailRewards(Int32 mailId, IEnumerable<MailReward> rewards);

    public Task<Tuple<ErrorCode, List<MailReward>>> LoadMailReward(Int32 mailId, String reward);

    public Task<ErrorCode> DeleteMailReward(Int32 mailId);
}
