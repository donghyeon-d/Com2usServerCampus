using DungeonAPI.ModelDB;
using SqlKata.Execution;
using ZLogger;
using static DungeonAPI.ModelDB.MasterData;

namespace DungeonAPI.Services;

public partial class GameDb : IGameDb
{
    public async Task<ErrorCode> CreateDefaltItemsAsync(Int32 playerId)
    {
        var cols = new[] { "PlayerId", "ItemCode", "ItemCount", "Attack", "Defence", "Magic",
                "EnhanceLevel", "EnhanceTryCount", "IsDestructed", "IsDeleted"};
        List<object[]> defaltItems = MakeDefalutItems(playerId);
        try
        {
            var count = await _queryFactory.Query("Item").InsertAsync(cols, defaltItems);
            if (defaltItems.Count != count)
            {
                var rollbackErrorCode = await DeletePlayerAllItemsAsync(playerId);
                if (rollbackErrorCode != ErrorCode.None)
                {
                    _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, "RollBackError " + rollbackErrorCode.ToString());
                }
                return ErrorCode.DefaultItemCreateFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            var rollbackErrorCode = await DeletePlayerAllItemsAsync(playerId);
            if (rollbackErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, "RollBackError " + rollbackErrorCode.ToString());
            }
            return ErrorCode.DefaultItemCreateFailException;
        }
    }

    List<object[]> MakeDefalutItems(Int32 playerId)
    {
        List<object[]> items = new List<object[]>();
        var list = MasterDataDb.s_baseItem;

        foreach (var item in list)
        {
            if (item.Name == "작은 칼"
                || item.Name == "나무 방패"
                || item.Name == "보통 모자")
            {
                items.Add(new object[]
                {
                    playerId,
                    item.Code,
                    1,
                    item.Attack,
                    item.Defence,
                    item.Magic,
                    0,
                    0,
                    false,
                    false
                });
            }

        }
        return items;
    }

    public async Task<Tuple<ErrorCode, Int32>> AddItemToPlayerItemList(Int32 playerId, Item item)
    {
        try
        {
            if (Util.ItemAttribute.IsEquipment(item.ItemCode))
            {
                var (addOneItemErrorCode, itemId) = await AddOneItem(item);
                if (addOneItemErrorCode != ErrorCode.None)
                {
                    return new Tuple<ErrorCode, Int32>(addOneItemErrorCode, -1);
                }
                return new Tuple<ErrorCode, Int32>(ErrorCode.None, itemId);
            }
            else
            {
                var (AddStackedItemErrorCode, itemId) = await AddStackItem(playerId, item);
                if (AddStackedItemErrorCode != ErrorCode.None)
                {
                    return new Tuple<ErrorCode, Int32>(AddStackedItemErrorCode, -1);
                }
                return new Tuple<ErrorCode, Int32>(ErrorCode.None, itemId);
            }
        }
        catch ( Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new Tuple<ErrorCode, Int32>(ErrorCode.AcquiredItemFailException, -1);
        }
    }

    async Task<Tuple<ErrorCode, Int32>> AddOneItem(Item item)
    {
        try
        {
            int itemId = await _queryFactory.Query("Item").InsertGetIdAsync<Int32>(item);
            if (itemId == 0)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.AddOneItemFail, -1);
            }
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, itemId);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new Tuple<ErrorCode, Int32>(ErrorCode.AddOntItemFailException, -1);
        }
    }

    async Task<Tuple<ErrorCode, Int32>>AddStackItem(Int32 playerId, Item item)
    {
        try
        {
            var result = await _queryFactory.Query("Item").Where("PlayerId", playerId).GetAsync<Item>();
            if (result == null)
            {
                return new (ErrorCode.AddStackItemGetFail, -1);
            }
            var playerItems = result.ToList<Item>();

            var masterDataItem = MasterDataDb.s_baseItem.Find(i => i.Code == item.ItemCode);
            if (masterDataItem == null)
            {
                return new (ErrorCode.NotFoundMasterDataItemAtAddStackItem, -1);
            }

            var sameAtrributItems = playerItems.FindAll(i => i.ItemCode == item.ItemCode);
            var canAddedItem = sameAtrributItems.Find(i => i.ItemCount < masterDataItem.MaxStack);

            if (canAddedItem != null)
            {
                var (AddStackItemErrorCode, newItemId) = await AddStackItemWithDivision(canAddedItem, item, masterDataItem);
                if (AddStackItemErrorCode != ErrorCode.None)
                {
                    return new (AddStackItemErrorCode, -1);
                }
                return new (ErrorCode.None, newItemId);
            }
            else
            {
                var (insertItemErrorCode, itemId) = await AddOneItem(item);
                if (insertItemErrorCode != ErrorCode.None)
                {
                    return new (insertItemErrorCode, -1);
                }
                return new (ErrorCode.None, itemId);
            }
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new (ErrorCode.AddStackItemFailException, -1);
        }
    }

    async Task<Tuple<ErrorCode, Int32>> AddStackItemWithDivision(Item DbItem, Item addItem, BaseItem schemaItem)
    {
        int sumCount = DbItem.ItemCount + addItem.ItemCount;
        if (sumCount > schemaItem.MaxStack)
        {
            DbItem.ItemCount = schemaItem.MaxStack;
            addItem.ItemCount = sumCount - schemaItem.MaxStack;
            
            var(insertItemErrorCode, itemId) = await AddOneItem(addItem);
            if (insertItemErrorCode != ErrorCode.None)
            {
                return new (insertItemErrorCode, -1);
            }

            ErrorCode UpdateErrorCode = await UpdateItemAsync(DbItem);
            if (UpdateErrorCode != ErrorCode.None)
            {
                await DeleteItem(itemId);
                return new (UpdateErrorCode, -1);
            }

            return new (ErrorCode.None, itemId);
        }
        else
        {
            DbItem.ItemCount = sumCount;
            ErrorCode UpdateErrorCode = await UpdateItemAsync(DbItem);
            if (UpdateErrorCode != ErrorCode.None)
            {
                return new (UpdateErrorCode, -1);
            }
            
            return new (ErrorCode.None, DbItem.ItemId);
        }
    }

    public async Task<Tuple<ErrorCode, List<Item>?>> LoadPlayerItemListAsync(Int32 playerId)
    {
        try
        {
            var result = await _queryFactory.Query("Item").Where("PlayerId", playerId).GetAsync<Item>();
            List<Item> items = result.ToList();
            return new (ErrorCode.None, items);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new (ErrorCode.LoadAllItemsFailException, null);
        }
    }

    public async Task<ErrorCode> DeleteItem(Int32 itemId)
    {
        try
        {
            int count = await _queryFactory.Query("Item")
                                            .Where("ItemId", itemId)
                                            .DeleteAsync();

            if (count != 1)
            {
                return ErrorCode.DeleteItemFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.DeleteItemFailException;
        }
    }

    public async Task<ErrorCode> DeletePlayerAllItemsAsync(Int32 playerId)
    {
        try
        {
            int count = await _queryFactory.Query("Item")
                                            .Where("PlayerId", playerId)
                                            .DeleteAsync();
            if (count != 1)
            {
                return ErrorCode.DeletePlayerAllItemsFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.DeletePlayerAllItemsFailException;
        }
    }

    public async Task<Tuple<ErrorCode, Item?>> LoadItemByItemId(Int32 itemId)
    {
        try
        {
            var item = await _queryFactory.Query("Item")
                                            .Where("ItemId", itemId)
                                            .FirstOrDefaultAsync<Item>();
            if (item is null)
            {
                return new (ErrorCode.LoadItemNotFound, null);
            }
            return new (ErrorCode.None, item);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new (ErrorCode.LoadItemFailException, null);
        }
    }

    public async Task<ErrorCode> UpdateItemAsync(Item item)
    {
        try
        {
            int count = await _queryFactory.Query("Item")
                                            .Where("itemId", item.ItemId)
                                            .UpdateAsync(item);
            if (count != 1)
            {
                return ErrorCode.UpdateItemFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.UpdateItemFailException;
        }
    }
}

