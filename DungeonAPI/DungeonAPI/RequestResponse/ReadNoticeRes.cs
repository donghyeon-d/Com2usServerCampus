using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class ReadNoticeRes : AuthPlayerResponse
{
    public List<Notification>? Notices { get; set; } = null;
}

