using System;
using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class ReceiveMailRewardRes
{
    [Required]
    public List<MailReward>? MailRewards { get; set; } = null;

    [Required]
    public ErrorCode Result { get; set; } = ErrorCode.None;
}

