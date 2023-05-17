using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface ICompletedDungeonDb
{
    public Task<ErrorCode> AddCompletedDungeon(Int32 playerId, Int32 stageCode);
    public Task<ErrorCode> DeleteWhenFail(Int32 playerId, Int32 stageCode);
    public Task<Tuple<ErrorCode, List<CompletedDungeon>?>> ReadCompleteList(Int32 playerId);

}
