using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.MessageBody;

public class LoadNoticeRes
{
    public List<Notification> notices { get; set; }
    public ErrorCode ErrorCode { get; set; }
}

