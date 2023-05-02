using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IInventory
{
    public Task<ErrorCode> CreateDefaltItemsAsync(Int32 userId);

    //public Task<Tuple<ErrorCode, Int32>> CreateItemAsync(Int32 userId, Int32 itemCode);

    public Task<Tuple<ErrorCode, List<Item>>> LoadAllItemsAsync(Int32 userId);

    //public Task<ErrorCode> UpdateItemAsync(Item item);

    //public Task<ErrorCode> DeleteItemAsync(Int32 userId, Int32 ItemId);
}

