using DungeonAPI.Enum;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IMemoryDb
{
    public Task<ErrorCode> CreatePlayerInfo(string email, string authToken, Int32 playerId);
    public Task<ErrorCode> ChangeUserStatus(string email, PlayerStatus status, Int32 stageCode = 0);
    public Task<ErrorCode> UpdateUserStatus(string email, PlayerInfo value);
    public Task<ErrorCode> DeletePlayer(string email);
    public Task<Tuple<ErrorCode, PlayerInfo?>> LoadPlayer(string email);
    public Task<Tuple<ErrorCode, InDungeon?>> GetDungeonInfo(string email);
    public Task<ErrorCode> SetDungeonInfo(string email, InDungeon dungeonInfo);
    public Task<ErrorCode> DeleteDungeonInfo(string email);
}

