﻿using System;
using System.Reflection.PortableExecutable;
using DungeonAPI.ModelDB;
using DungeonAPI.Services;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using static DungeonAPI.Models.MasterData;

namespace DungeonAPI.Services;

public class Inventory : GameDb, IInventory
{
    readonly ILogger<User> _logger;

    public Inventory(ILogger<User> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
    {
        _logger = logger;
	}

    public async Task<ErrorCode> CreateDefaltItemsAsync(Int32 userId)
    {
        // 아이템 인스턴스 만들기
        var cols = new[] { "UserId", "ItemCode", "ItemCount", "Attack", "Defence", "Magic",
                "EnhanceLevel", "RemainingEnhanceCount", "Destructed"};
        List<object[]> defaltItems = MakeDefalutItems(userId);
        try
        {
            var count = await _queryFactory.Query("inventory").InsertAsync(cols, defaltItems);
            if (defaltItems.Count != count)
            {
                // 롤백
                return ErrorCode.DefaultItemCreateFail;
            }
            return ErrorCode.None;
        }
        catch ( Exception e )
        {
            // 로그
            return ErrorCode.DefaultItemCreateFailException;
        }
        finally
        {
            Dispose();
        }
    }

    List<object[]> MakeDefalutItems(Int32 userId)
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
                    userId,
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
}

