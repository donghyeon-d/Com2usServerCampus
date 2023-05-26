using DungeonAPI.Enum;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IMemoryDb
{
    // Player Info
    public Task<ErrorCode> CreatePlayerInfo(Int32 playerId, string authToken);
    public Task<ErrorCode> ChangeUserStatus(Int32 playerId, PlayerStatus status, Int32 stageCode = 0);
    public Task<ErrorCode> UpdateUserStatus(Int32 playerId, PlayerInfo value);
    public Task<ErrorCode> DeletePlayer(Int32 playerId);
    public Task<Tuple<ErrorCode, PlayerInfo?>> LoadPlayer(Int32 playerId);

    // InDungeon Info
    public Task<Tuple<ErrorCode, InDungeon?>> GetDungeonInfo(Int32 playerId);
    public Task<ErrorCode> SetDungeonInfo(Int32 playerId, InDungeon dungeonInfo);
    public Task<ErrorCode> DeleteDungeonInfo(Int32 playerId);

    // Notice
    public Task<ErrorCode> CreateNotification(string title, string content, DateTime? dateTime = null);
    public Task<Tuple<ErrorCode, List<Notification>?>> ReadNotificationList();
}

