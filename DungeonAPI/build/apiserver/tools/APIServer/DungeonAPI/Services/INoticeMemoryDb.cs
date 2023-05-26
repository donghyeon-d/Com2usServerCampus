using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface INoticeMemoryDb
{
    public Task<ErrorCode> CreateNotification(string title, string content, DateTime? dateTime = null);
    public Task<Tuple<ErrorCode, List<Notification>?>> ReadNotificationList();

}

