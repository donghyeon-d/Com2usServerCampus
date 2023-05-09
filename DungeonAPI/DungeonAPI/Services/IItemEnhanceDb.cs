using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IItemEnhanceDb
{
    public Task<Tuple<ErrorCode, Item>> EnhancePlayerItem(Int32 itemId, Int32 playerId);
}
