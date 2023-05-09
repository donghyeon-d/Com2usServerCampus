using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class LoadNoticeRes
{
    public List<Notification> Notices { get; set; }
    public ErrorCode ErrorCode { get; set; }
}

