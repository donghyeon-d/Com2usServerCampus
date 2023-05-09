using System;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;

namespace DungeonAPI.Services;

public class ItemEnhanceDb : GameDb, IItemEnhanceDb
{
	readonly ILogger<ItemEnhanceDb> _logger;
	readonly IItemDb _itemDb;

	public ItemEnhanceDb(ILogger<ItemEnhanceDb> logger, IOptions<DbConfig> dbConfig,
        IItemDb itemDb)
		:base(logger, dbConfig)
	{
		_logger = logger;
		_itemDb = itemDb;
	}

    public async Task<Tuple<ErrorCode, Item>> EnhancePlayerItem(Int32 itemId)
	{
		var (LoadItemErrorCode, item) = await _itemDb.LoadItemByItemId(itemId);
		if (LoadItemErrorCode != ErrorCode.None)
		{
			return new Tuple<ErrorCode, Item>(LoadItemErrorCode, null);
        }

		if (CanEnhanceItem(item) == false)
		{
			return new Tuple<ErrorCode, Item>(ErrorCode.UnenhanceableItem, null);
        }

        Item resultItem = TryEnhanceItem(3, 10, item);

		var updateItemErrorCode = await _itemDb.UpdateItemAsync(resultItem);
		if (updateItemErrorCode != ErrorCode.None)
		{
			return new Tuple<ErrorCode, Item>(updateItemErrorCode, null);
		}
        return new Tuple<ErrorCode, Item>(ErrorCode.None, resultItem);
    }

	bool CanEnhanceItem(Item item)
	{
		if (item.RemainingEnhanceCount == 0 || item.IsDestructed == 1)
		{
			return false;
		}

		var baseItem = MasterDataDb.s_baseItem.Find(masterDataItem => masterDataItem.Code == item.ItemMasterDataCode);
		if (baseItem is null ||
			baseItem.Attribute != 1 || baseItem.Attribute != 2)
		{
			return false;
		}

		return true;
	}

	Item TryEnhanceItem(int numerator, int denominator, Item item)
	{
        if (IsEnhanceSuccess(numerator, denominator))
		{
			item.Attack = Convert.ToInt32(item.Attack * 1.1);
            item.Defence = Convert.ToInt32(item.Defence * 1.1);
            item.Magic = Convert.ToInt32(item.Magic * 1.1);
			item.EnhanceLevel = Convert.ToByte(item.EnhanceLevel + 1);
            item.RemainingEnhanceCount = Convert.ToByte(item.RemainingEnhanceCount - 1);
        }
		else
		{
			item.IsDestructed = 1;
        }
		return item;
    }

	bool IsEnhanceSuccess(int numerator, int denominator)
	{
        Random rand = new Random();
        int randomInt = rand.Next(denominator);

		if (randomInt < numerator)
		{
			return true;
		}
		return false;
    }
}

