using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface INotice
{
    // 후입 선출

    // push left
    public Task<ErrorCode> CreateNotification(string title, string content, DateTime? dateTime = null);

    // pop all data
    public Task<Tuple<ErrorCode, List<Notification>>> LoadAllNotification();

}

