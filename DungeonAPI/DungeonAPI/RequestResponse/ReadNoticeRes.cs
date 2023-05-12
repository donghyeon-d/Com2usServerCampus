using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class ReadNoticeRes
{
    public List<Notification>? Notices { get; set; } = null;
    public ErrorCode ErrorCode { get; set; } = ErrorCode.None;
}

