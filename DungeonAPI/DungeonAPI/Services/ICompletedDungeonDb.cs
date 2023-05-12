using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface ICompletedDungeonDb
{
    public Task<ErrorCode> AddCompletedDungeon(Int32 playerId, String thema, Int32 step);
    public Task<ErrorCode> DeleteWhenFail(Int32 playerId, String thema, Int32 step);
    public Task<Tuple<ErrorCode, List<CompletedDungeon>?>> ReadCompleteList(Int32 playerId);
    public Task<Tuple<ErrorCode, List<CompletedDungeon>?>> ReadCompleteThemaList(Int32 playerId, String thema);
}
