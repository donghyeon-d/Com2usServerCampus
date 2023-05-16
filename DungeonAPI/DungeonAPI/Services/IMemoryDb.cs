using DungeonAPI.Enum;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IMemoryDb
{
    public Task<ErrorCode> CreatePlayerInfo(string email, string authToken, Int32 playerId);
    public Task<ErrorCode> ChangeUserStatus(string email, PlayerStatus status, Int32 stageCode = 0);
    public Task<ErrorCode> UpdateUserStatus(string email, PlayerInfo value);
    public Task<ErrorCode> DeleteAuthUserAsync(string email);
    public Task<Tuple<ErrorCode, PlayerInfo?>> LoadAuthUser(string email);
    public Task<Tuple<ErrorCode, List<FarmingItem>?>> GetFarmingItemList(string email);
    public Task<Tuple<ErrorCode, List<KillNPC>?>> GetKillNPCList(string email);
    public Task<ErrorCode> SetFarmingItemList(string email,  List<FarmingItem> items);
    public Task<ErrorCode> SetKillNPCList(string email, List<KillNPC> NPCs);
    public Task<ErrorCode> DeleteDungeonInfo(string email);

}

