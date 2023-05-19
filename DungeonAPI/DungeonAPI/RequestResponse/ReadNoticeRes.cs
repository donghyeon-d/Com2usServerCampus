using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class ReadNoticeRes
{
    public List<Notification>? Notices { get; set; } = null;
    public ErrorCode Result { get; set; } = ErrorCode.None;
}

