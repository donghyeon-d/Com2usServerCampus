using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using DungeonAPI.ModelsDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using static DungeonAPI.ModelDB.MasterData;

namespace DungeonAPI.Services;

public class ItemDb : GameDb, IItemDb
{
    readonly ILogger<ItemDb> _logger;

    public ItemDb(ILogger<ItemDb> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
    {
        _logger = logger;
    }

    public async Task<ErrorCode> CreateDefaltItemsAsync(Int32 playerId)
    {
        Open();
        // 아이템 인스턴스 만들기
        var cols = new[] { "PlayerId", "ItemMasterDataCode", "ItemCount", "Attack", "Defence", "Magic",
                "EnhanceLevel", "RemainingEnhanceCount", "IsDestructed"};
        List<object[]> defaltItems = MakeDefalutItems(playerId);
        try
        {
            var count = await _queryFactory.Query("Item").InsertAsync(cols, defaltItems);
            if (defaltItems.Count != count)
            {
                // 롤백
                await DeletePlayerAllItemsAsync(playerId);
                return ErrorCode.DefaultItemCreateFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            // 로그
            _logger.LogError(e.Message);
            await DeletePlayerAllItemsAsync(playerId);
            return ErrorCode.DefaultItemCreateFailException;
        }
        finally
        {
            Dispose();
        }
    }

    List<object[]> MakeDefalutItems(Int32 playerId)
    {
        List<object[]> items = new List<object[]>();
        var list = MasterDataDb.s_item;

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
                    item.EnhanceMaxCount,
                    0
                });
            }

        }
        return items;
    }

    public async Task<Tuple<ErrorCode, Int32>> AcquiredItem(Int32 playerId, Item item)
    {
        try
        {
            if (IsEquipment(item))
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
                var (AddStackedItemErrorCode, itemId) = await AddCanStackItemDividedByNumber(playerId, item);
                if (AddStackedItemErrorCode != ErrorCode.None)
                {
                    return new Tuple<ErrorCode, Int32>(AddStackedItemErrorCode, -1);
                }
                return new Tuple<ErrorCode, Int32>(ErrorCode.None, itemId);
            }
        }
        catch ( Exception e)
        {
            return new Tuple<ErrorCode, Int32>(ErrorCode.AcquiredItemFailException, -1);
        }
    }

    async Task<Tuple<ErrorCode, Int32>> AddOneItem(Item item)
    {
        try
        {
            int itemId = await _queryFactory.Query("Item").InsertGetIdAsync<Int32>(item);
            if (itemId != 1)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.AddOneItemFail, -1);
            }
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, itemId);
        }
        catch (Exception e)
        {
            return new Tuple<ErrorCode, Int32>(ErrorCode.AddOntItemFailException, -1);
        }
    }

    async Task<Tuple<ErrorCode, Int32>>AddCanStackItemDividedByNumber(Int32 playerId, Item item)
    {
        try
        {
            var result = await _queryFactory.Query("Item").Where("PlayerId", playerId).GetAsync<Item>();
            if (result == null)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.AddStackItemGetFail, -1);
            }
            var playerItems = result.ToList<Item>();

            var masterDataItem = MasterDataDb.s_item.Find(i => i.Code == item.ItemMasterDataCode);
            if (masterDataItem == null)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.NotFoundMasterDataItemAtAddStackItem, -1);
            }

            var sameAtrributItems = playerItems.FindAll(i => i.ItemMasterDataCode == item.ItemMasterDataCode);
            var canAddedItem = sameAtrributItems.Find(i => i.ItemCount < masterDataItem.MaxStack);
            if (canAddedItem != null)
            {
                int sumCount = canAddedItem.ItemCount + item.ItemCount;
                if (sumCount > masterDataItem.MaxStack)
                {
                    canAddedItem.ItemCount = masterDataItem.MaxStack;
                    item.ItemCount = sumCount - masterDataItem.MaxStack;
                    var (insertItemErrorCode, itemId) = await AddOneItem(item);
                    if (insertItemErrorCode != ErrorCode.None)
                    {
                        return new Tuple<ErrorCode, Int32>(insertItemErrorCode, -1);
                    }
                    ErrorCode UpdateErrorCode = await UpdateItemAsync(canAddedItem);
                    if (UpdateErrorCode != ErrorCode.None)
                    {
                        await DeleteItemByItemId(itemId);
                        return new Tuple<ErrorCode, Int32>(UpdateErrorCode, -1);
                    }
                    return new Tuple<ErrorCode, Int32>(ErrorCode.None, itemId);
                }
                else
                {
                    canAddedItem.ItemCount = sumCount;
                    ErrorCode UpdateErrorCode = await UpdateItemAsync(canAddedItem);
                    if (UpdateErrorCode != ErrorCode.None)
                    {
                        return new Tuple<ErrorCode, Int32>(UpdateErrorCode, -1);
                    }
                    return new Tuple<ErrorCode, Int32>(ErrorCode.None, canAddedItem.ItemId); // TODO: WRONG RETURN
                }
            }
            else
            {
                var (insertItemErrorCode, itemId) = await AddOneItem(item);
                if (insertItemErrorCode != ErrorCode.None)
                {
                    return new Tuple<ErrorCode, Int32>(insertItemErrorCode, -1);
                }
                return new Tuple<ErrorCode, Int32>(ErrorCode.None, itemId);
            }
        }
        catch (Exception e)
        {
            return new Tuple<ErrorCode, Int32>(ErrorCode.AddStackItemFailException, -1);
        }
    }

    bool IsEquipment(Item item)
    {
        var itemKind = MasterDataDb.s_item.Find(i => i.Code == item.ItemMasterDataCode);
        if (itemKind.Attribute == 1 || itemKind.Attribute == 2 || itemKind.Attribute == 3)
        {
            return true;
        }
        return false;
    }

    bool IsGold(Item item)
    {
        var itemKind = MasterDataDb.s_item.Find(i => i.Code == item.ItemMasterDataCode);
        if (itemKind.Attribute == 5)
        {
            return true;
        }
        return false;
    }

    bool IsComsumableItem(Item item)
    {
        var itemKind = MasterDataDb.s_item.Find(i => i.Code == item.ItemMasterDataCode);
        if (itemKind.Attribute == 4)
        {
            return true;
        }
        return false;
    }

    public async Task<Tuple<ErrorCode, List<Item>>> LoadAllItemsAsync(Int32 playerId)
    {
        try
        {
            var result = await _queryFactory.Query("Item").Where("PlayerId", playerId).GetAsync<Item>();
            List<Item> items = result.ToList();
            return new Tuple<ErrorCode, List<Item>>(ErrorCode.None, items);
        }
        catch (Exception e)
        {
            // TODO : log
            _logger.LogError(e.Message);
            return new Tuple<ErrorCode, List<Item>>(ErrorCode.LoadAllItemsFailException, null);
        }
    }

    async Task<ErrorCode> DeleteItemByItemId(Int32 itemId)
    {
        Open();
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
            // TODO : log
            return ErrorCode.DeleteItemFailException;
        }
        finally
        {
            Dispose();
        }
    }

    public async Task<ErrorCode> DeletePlayerAllItemsAsync(Int32 playerId)
    {
        Open();
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
            // TODO : log
            return ErrorCode.DeletePlayerAllItemsFailException;
        }
        finally
        {
            Dispose();
        }
    }

    public async Task<Tuple<ErrorCode, Item>> LoadItemByItemId(Int32 itemId)
    {
        Open();
        try
        {
            var item = await _queryFactory.Query("Item")
                                            .Where("ItemId", itemId)
                                            .FirstOrDefaultAsync<Item>();
            if (item is null)
            {
                return new Tuple<ErrorCode, Item>(ErrorCode.LoadItemNotFound, null);
            }
            return new Tuple<ErrorCode, Item>(ErrorCode.None, item);
        }
        catch (Exception e)
        {
            // TODO : log
            return new Tuple<ErrorCode, Item>(ErrorCode.LoadItemFailException, null);
        }
        finally
        {
            Dispose();
        }
    }

    public async Task<ErrorCode> UpdateItemAsync(Item item)
    {
        Open();
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
            // TODO : log
            return ErrorCode.UpdateItemFailException;
        }
        finally
        {
            Dispose();
        }
    }
}

