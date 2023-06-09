﻿using System;
using DungeonAPI.Enum;
using DungeonAPI.ModelDB;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class EnhanceItemController : ControllerBase
{
    readonly ILogger<EnhanceItemController> _logger;
    readonly IGameDb _gameDb;

    public EnhanceItemController(ILogger<EnhanceItemController> logger,
        IGameDb gameDb)
	{
        _logger = logger;
        _gameDb = gameDb;
    }

    [HttpPost]
    public async Task<EnhanceItemRes> EnhanceItemThenGetResultItem(EnhanceItemReq request)
    {
        EnhanceItemRes response = new EnhanceItemRes();

        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        var (EnhancePlayerItemErrorCode, resultItem) =
            await EnhancePlayerItem(request.ItemId, player.Id);

        response.Result = EnhancePlayerItemErrorCode;
        response.ResultItem = resultItem;
        _logger.ZLogInformationWithPayload(new { PlayerId = player.Id, RequestItemId = request.ItemId, 
                                    ResultItem = response.ResultItem }, response.Result.ToString());

        return response;
    }

    async Task<Tuple<ErrorCode, Item?>> EnhancePlayerItem(Int32 itemId, Int32 playerId)
    {
        var (LoadItemErrorCode, item) = await _gameDb.LoadItemByItemId(itemId);
        if (LoadItemErrorCode != ErrorCode.None || item is null)
        {
            return new(LoadItemErrorCode, null);
        }

        var CheckEnhanceResult = CheckCanEnhanceItem(playerId, item);
        if (CheckEnhanceResult != ErrorCode.None)
        {
            return new(CheckEnhanceResult, null);
        }

        Item resultItem = TryEnhanceItem((int)EnhancePercent.EquiptNumerator, 
                                (int)EnhancePercent.EquiptDenominator, item);

        var updateItemErrorCode = await _gameDb.UpdateItemAsync(resultItem);
        if (updateItemErrorCode != ErrorCode.None)
        {
            return new(updateItemErrorCode, null);
        }

        return new(ErrorCode.None, resultItem);
    }


    ErrorCode CheckCanEnhanceItem(Int32 playerId, Item item)
    {
        if (playerId != item.PlayerId)
        {
            return ErrorCode.WrongItemOwner;
        }

        var baseItem = MasterDataDb.s_baseItem.Find(i => i.Code == item.ItemCode);
        if (baseItem == null)
        {
            return ErrorCode.InvalidItemCode;
        }

        if (item.EnhanceTryCount == baseItem.EnhanceMaxCount)
        {
            return ErrorCode.TryMoreThanMaxCount;
        }

        if (item.IsDestructed == true)
        {
            return ErrorCode.DestructedItem;
        }

        if (baseItem.Attribute != (int)ItemAttribute.Weapon
            && baseItem.Attribute != (int)ItemAttribute.Armor)
        {
            return ErrorCode.UnenhanceableItem;
        }

        return ErrorCode.None;
    }

    Item TryEnhanceItem(int numerator, int denominator, Item item)
    {
        if (IsEnhanceSuccess(numerator, denominator))
        {
            item.Attack = Convert.ToInt32(item.Attack * 1.1);
            item.Defence = Convert.ToInt32(item.Defence * 1.1);
            item.Magic = Convert.ToInt32(item.Magic * 1.1);
            item.EnhanceLevel = Convert.ToByte(item.EnhanceLevel + 1);
            item.EnhanceTryCount = Convert.ToByte(item.EnhanceTryCount + 1);
        }
        else
        {
            item.IsDestructed = true;
        }
        return item;
    }

    bool IsEnhanceSuccess(int numerator, int denominator)
    {
        Random rand = new();
        int randomInt = rand.Next(denominator);

        if (randomInt < numerator)
        {
            return true;
        }
        return false;
    }
}

