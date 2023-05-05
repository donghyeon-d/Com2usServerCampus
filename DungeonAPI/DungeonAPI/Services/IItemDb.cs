using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IItemDb
{
    public Task<ErrorCode> CreateDefaltItemsAsync(Int32 playerId);

    //public Task<Tuple<ErrorCode, Int32>> CreateItemAsync(Int32 playerId, Int32 itemCode);

    public Task<Tuple<ErrorCode, List<Item>>> LoadAllItemsAsync(Int32 playerId);

    public Task<ErrorCode> UpdateItemAsync(Item item);

    public Task<ErrorCode> DeletePlayerAllItemsAsync(Int32 playerId);

    public Task<Tuple<ErrorCode, Item>> LoadItemByItemId(Int32 itemId);
}

