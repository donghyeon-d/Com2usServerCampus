﻿using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface IMailRewardDb
{
    public Task<ErrorCode> CreateMailRewards(Int32 mailId, List<MailReward> rewards);
    public Task<Tuple<ErrorCode, List<MailReward>>> LoadMailRewards(Int32 mailId);
    public Task<ErrorCode> DeleteMailReward(Int32 mailId);
    public Task<Tuple<ErrorCode, List<MailReward>>> ReceiveMailRewards(Int32 playerId, Int32 mailId);
}

