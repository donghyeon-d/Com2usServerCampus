﻿using DungeonAPI.Services;
using DungeonAPI.RequestResponse;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;
using static Humanizer.In;
using DungeonAPI.Enum;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StageCompleteController : ControllerBase
{
    readonly ILogger<StageCompleteController> _logger;
    readonly IMemoryDb _memoryDb;
    readonly IItemDb _itemDb;
    readonly IPlayerDb _playerDb;
    readonly ICompletedDungeonDb _completedDungeonDb;

    public StageCompleteController(ILogger<StageCompleteController> logger,
    IMemoryDb memoryDb, IItemDb itemDb, IPlayerDb playerDb, ICompletedDungeonDb completedDungeonDb)
    {
        _logger = logger;
        _memoryDb = memoryDb;
        _itemDb = itemDb;
        _playerDb = playerDb;
        _completedDungeonDb = completedDungeonDb;
    }

    [HttpPost]
    public async Task<StageCompleteRes> StageCompleteThenApplyResult(StageCompleteReq request)
    {
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        if (IsValidRequest(player.Status, player.currentStage, request.Stage) == false)
        {
            await SetExitDungeon(request.Email);
            return new StageCompleteRes() { Result = ErrorCode.StageCompleteInvalidPlayerStatus };
        }

        var saveCompletedDungeonErrorCode = await SaveCompletedDungeon(player.Id, player.currentStage);
        if (saveCompletedDungeonErrorCode != ErrorCode.None)
        {
            return new StageCompleteRes() { Result = saveCompletedDungeonErrorCode };
        }

        var (saveRewardErrorCode, farmingItemList) = await SaveReward(request.Email, player.Id, player.currentStage);
        if (saveRewardErrorCode != ErrorCode.None)
        {
            return new StageCompleteRes() { Result = saveRewardErrorCode };
        }

        var changePlayerStatusErrorCode = await ChangePlayerStatusToLogin(request.Email);
        if (changePlayerStatusErrorCode != ErrorCode.None)
        {
            await SetLogOff(request.Email);
            return new StageCompleteRes() { Result = changePlayerStatusErrorCode };
        }

        return new StageCompleteRes() { Result = ErrorCode.None, RewardList = farmingItemList };
    }

     async Task<ErrorCode> SaveCompletedDungeon(Int32 playerId, Int32 playerStage)
    {
        var addCompletedDungeonErrorCoed =  await _completedDungeonDb.AddCompletedDungeon(playerId, playerStage);
        if (addCompletedDungeonErrorCoed != ErrorCode.None)
        {
            return addCompletedDungeonErrorCoed;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> ChangePlayerStatusToLogin(string email)
    {
        var changeUserStatusErrorCode = await _memoryDb.ChangeUserStatus(email, PlayerStatus.LogIn);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            return changeUserStatusErrorCode;
        }

        var deleteDungeonInfoErrorCode = await _memoryDb.DeleteDungeonInfo(email);
        if (deleteDungeonInfoErrorCode != ErrorCode.None
            && deleteDungeonInfoErrorCode != ErrorCode.DeleteFarmingItemListNotExist)
        {
            return deleteDungeonInfoErrorCode;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> SetLogOff(string email)
    {
        var changeUserStatusErrorCode = await _memoryDb.ChangeUserStatus(email, PlayerStatus.LogOff);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            // TODO : rollback error log
            return changeUserStatusErrorCode;
        }

        return ErrorCode.None;
    }

    async Task<Tuple<ErrorCode, List<FarmingItem>?>> SaveReward(string email, Int32 playerId, Int32 stage)
    {
        var (getDungeonInfoErrorCode, dungeonInfo) = await _memoryDb.GetDungeonInfo(email);
        if (getDungeonInfoErrorCode != ErrorCode.None || dungeonInfo is null)
        {
            return new(getDungeonInfoErrorCode, null);
        }

        var (saveItemErrorCode, itemIdList) = await SaveItemToInventory(playerId, dungeonInfo.ItemList);
        if (saveItemErrorCode != ErrorCode.None || itemIdList is null)
        {
            return new(saveItemErrorCode, null);
        }

        int totalExp = SumKillNPCListExp(dungeonInfo.KillNPCList, stage);

        var saveMoneyAndExpErrorCode = await SaveMoneyAndExpToPlayer(playerId, totalExp, dungeonInfo.ItemList);
        if (saveMoneyAndExpErrorCode != ErrorCode.None)
        {
            await RollbackItems(itemIdList);
            return new(saveMoneyAndExpErrorCode, null);
        }

        return new(ErrorCode.None, dungeonInfo.ItemList);
    }

    async Task<Tuple<ErrorCode, List<int>?>> SaveItemToInventory(Int32 playerId, List<FarmingItem> farmingList)
    {
        if (farmingList is null || IsEmptyItemList(farmingList))
        {
            return new(ErrorCode.None, new List<int>());
        }

        var ItemList = MakeItemList(playerId, farmingList);

        List<int> itemIdList = new();
        foreach(var item in ItemList)
        {
            var (addItemErrorCode, itemId) =
                await _itemDb.AddItemToPlayerItemList(playerId, item);
            if (addItemErrorCode != ErrorCode.None)
            {
                await RollbackAddedItem(itemIdList);
                return new(addItemErrorCode, null);
            }
        }

        return new(ErrorCode.None, itemIdList);
    }
    
    List<Item> MakeItemList(Int32 playerId, List<FarmingItem> farmingItemList)
    {
        List<Item> itemList = new();

        foreach(var farmingItem in farmingItemList)
        {
            var item = Item.InitItem(playerId, farmingItem.ItemCode, farmingItem.Count);
            if(Util.ItemAttribute.IsGold(item.ItemCode) == false)
            {
                itemList.Add(item);
            }
        }

        return itemList;
    }

    async Task RollbackAddedItem(List<int> itemIdList)
    {
        foreach (var itemId in itemIdList)
        {
            if (await _itemDb.DeleteItem(itemId) != ErrorCode.None)
            {
                // TODO : Rollback Error Log
            }
        }
    }

    int SumKillNPCListExp(List<KillNPC> NPCList, Int32 stage)
    {
        int totalExp = 0;

        foreach(var killNPC in NPCList)
        {
            // 앞에서 NPC유효성 검사 이미 했기 때문에 findNPC가 null일 수 없음
            var findNPC = MasterDataDb.s_stageAttackNPC.Find(NPC => 
                    NPC.NPCCode == killNPC.NPCCode && NPC.StageCode == stage);
            totalExp += findNPC.Exp * killNPC.Count;
        }

        return totalExp;
    }

    async Task<ErrorCode> SaveMoneyAndExpToPlayer(Int32 playerId, int totalExp, List<FarmingItem>? farmingList)
    {
        var (saveMoneyErrorCode, moneyCount) = await SaveMoney(playerId, farmingList);
        if (saveMoneyErrorCode != ErrorCode.None)
        {
            return saveMoneyErrorCode;
        }

        var addExpErrorCode = await _playerDb.AddExp(playerId, totalExp);
        if(addExpErrorCode != ErrorCode.None)
        {
            await RollbackAddMoney(playerId, moneyCount);
            return addExpErrorCode;
        }

        return ErrorCode.None;
    }

    bool IsEmptyItemList(List<FarmingItem>? farmingList)
    {
        if (farmingList is null || farmingList.Count() == 0)
        {
            return true;
        }

        foreach(var item in farmingList)
        {
            if(item.Count != 0)
            {
                return false;
            }
        }

        return true;
    }

    async Task<Tuple<ErrorCode, int>> SaveMoney(Int32 playerId, List<FarmingItem>? farmingList)
    {
        if (IsEmptyItemList(farmingList))
        {
            return new(ErrorCode.None, 0);
        }

        FarmingItem? money = farmingList.Find(item => Util.ItemAttribute.IsGold(item.ItemCode));
        if (money is null || money.Count == 0)
        {
            return new(ErrorCode.None, 0);
        }

        var addMoneyErrorCode = await _playerDb.AddMoney(playerId, money.Count);
        if (addMoneyErrorCode != ErrorCode.None)
        {
            return new(addMoneyErrorCode, 0);
        }

        return new(ErrorCode.None, money.Count);
    }

    async Task RollbackAddMoney(Int32 playerId, int amount)
    {
        if (amount == 0)
        {
            return;
        }

        var addMoneyErrorCode =  await _playerDb.AddMoney(playerId, amount * -1);
        if (addMoneyErrorCode != ErrorCode.None)
        {
            //  Rollback error log
        }
    }

    bool IsValidRequest(string playerStatus, Int32 playerCurrentStage, Int32 requestStage)
    {
        if (playerStatus != PlayerStatus.DungeonPlay.ToString())
        {
            return false;
        }

        if (playerCurrentStage != requestStage)
        {
            return false;
        }

        if (MasterDataDb.s_stage.Find(stage => stage.StageCode == playerCurrentStage) == null)
        { 
            return false;
        }

        return true;
    }

    async Task SetExitDungeon(string email)
    {
        var changeUserStatusErrorCode
            = await _memoryDb.ChangeUserStatus(email, PlayerStatus.LogIn);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            // TODO: Rollback Error
        }

        var deleteDungeonInfoErrorCode = await _memoryDb.DeleteDungeonInfo(email);
        if (deleteDungeonInfoErrorCode != ErrorCode.None)
        {
            // TODO : Rollback Error
        }
    }

    async Task RollbackItems(List<int> itemIdList)
    {
        foreach (var itemId in itemIdList) 
        {
            var deleteItemErrorCode = await _itemDb.DeleteItem(itemId);
            if (deleteItemErrorCode != ErrorCode.None )
            {
                // TODO: rollback error log
            }
        }
    }
}


