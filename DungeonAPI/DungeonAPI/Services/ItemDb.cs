using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using DungeonAPI.ModelsDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;

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
        var cols = new[] { "PlayerId", "ItemMasterCode", "ItemCount", "Attack", "Defence", "Magic",
                "EnhanceLevel", "RemainingEnhanceCount", "Destructed"};
        List<object[]> defaltItems = MakeDefalutItems(playerId);
        try
        {
            var count = await _queryFactory.Query("item").InsertAsync(cols, defaltItems);
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


    public async Task<Tuple<ErrorCode, List<Item>>> LoadAllItemsAsync(Int32 playerId)
    {
        try
        {
            var result = await _queryFactory.Query("item").Where("PlayerId", playerId).GetAsync<Item>();
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

    public async Task<ErrorCode> DeletePlayerAllItemsAsync(Int32 playerId)
    {
        Open();
        try
        {
            int count = await _queryFactory.Query("item")
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
}

