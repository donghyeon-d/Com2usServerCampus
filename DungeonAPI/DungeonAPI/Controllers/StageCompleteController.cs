using DungeonAPI.Services;
using DungeonAPI.RequestResponse;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StageCompleteController : ControllerBase
{
    readonly ILogger<StageCompleteController> _logger;
    readonly IMemoryDb _memoryDb;
    readonly IItemDb _itemDb;
    readonly IPlayerDb _playerDb;

    public StageCompleteController(ILogger<StageCompleteController> logger,
    IMemoryDb memoryDb, IItemDb itemDb, IPlayerDb playerDb)
    {
        _logger = logger;
        _memoryDb = memoryDb;
        _itemDb = itemDb;
        _playerDb = playerDb;
    }

    [HttpPost]
    public async Task<StageCompleteRes> StageCompleteThenApplyResult(StageCompleteReq request)
    {
        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());
        string playerStatus = HttpContext.Items["PlayerStatus"].ToString();
        Int32 playerStage = int.Parse(HttpContext.Items["PlayerStage"].ToString());

        if (IsValidRequest(playerStatus, playerStage, request.Stage) == false)
        {
            await SetExitDungeon(request.Email);
            return new StageCompleteRes() { Result = ErrorCode.StageCompleteInvalidPlayerStatus };
        }

        var (checkKillAllNPCErrorCode, killedNPCList) = await CheckKillAllNPC(playerStage, request.Email);
        if (checkKillAllNPCErrorCode != ErrorCode.None || killedNPCList is null)
        {
            return new StageCompleteRes() { Result = checkKillAllNPCErrorCode };
        }

        var (checkFarmingItemErrorCode, farmingItemList) = await CheckFarmingItem(playerStage, request.Email);
        if (checkFarmingItemErrorCode != ErrorCode.None || farmingItemList is null)
        {
            return new StageCompleteRes() { Result = checkFarmingItemErrorCode };
        }

        var itemIdList = await SaveItemList(playerId, farmingItemList);
        if (itemIdList is null)
        {
            return new StageCompleteRes() { Result = ErrorCode.SaveStageRewardItemListFail };
        }

        var saveMoneyAndExpErrorCode = await SaveMoneyAndExp(playerId, farmingItemList, killedNPCList);
        if (saveMoneyAndExpErrorCode != ErrorCode.None)
        {
            await RollbackItems(itemIdList);
            return new StageCompleteRes() { Result = saveMoneyAndExpErrorCode };
        }

        // TODO: Redis에 던전 저장값 지워주기, 플레이어 상태 바꿔주기 


        return new StageCompleteRes() { Result = ErrorCode.None, RewardList = farmingItemList };
        }

    bool IsValidRequest(string playerStatus, Int32 playerCurrentStage, Int32 requestStage)
    {
        if (playerStatus == PlayerStatus.DungeonPlay.ToString())
        {
            return true;
        }

        if (playerCurrentStage != requestStage)
        {
            return false;
        }

        if (MasterDataDb.s_stage.Find(stage => stage.StageCode == playerCurrentStage) == null)
        { 
            return false;
        }

        return false;
    }

    async Task SetExitDungeon(string email)
    {
        var changeUserStatusErrorCode
            = await _memoryDb.ChangeUserStatus(email, PlayerStatus.LogIn);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            // TODO: Rollback Error
        }
    }

    async Task<Tuple<ErrorCode, List<KillNPC>?>> CheckKillAllNPC(Int32 playerStage, string email)
    {
        var (GetKillNPCErrorCode, killedNPCList) = await _memoryDb.GetKillNPCList(email);
        if (GetKillNPCErrorCode != ErrorCode.None || killedNPCList is null)
        {
            return new(GetKillNPCErrorCode, null);
        }

        if (IsAllNPCInStage(killedNPCList, playerStage) == false)
        {
            return new(ErrorCode.PlayerDontKillAllNPC, null);
        }

        return new(ErrorCode.None, killedNPCList);
    }

    bool IsAllNPCInStage(List<KillNPC> killedNPCList, Int32 playerStage)
    {
        var NPCListInstage = MasterDataDb.s_stageAttackNPC.FindAll(npc => npc.StageCode == playerStage);

        foreach (var npcInstage in NPCListInstage)
        {
            var result = killedNPCList.Find(killedNPC => killedNPC.NPCCode == npcInstage.NPCCode
                                           && killedNPC.Count == npcInstage.NPCCount);
            if (result is null)
            { 
                return false; 
            }
        }
        return true;
    }

    async Task<Tuple<ErrorCode, List<FarmingItem>?>> CheckFarmingItem(Int32 playerStage, string email)
    {
        var (getFarmingItemListErrorCode, farmingItemList) = await _memoryDb.GetFarmingItemList(email);
        if (getFarmingItemListErrorCode != ErrorCode.None || farmingItemList is null)
        {
            return new(getFarmingItemListErrorCode, null);
        }

        var invalidStageRewardErrorCode = IsValidStageReward(playerStage, farmingItemList);
        if (invalidStageRewardErrorCode != ErrorCode.None)
        {
            return new(invalidStageRewardErrorCode, null);
        }

        return new(ErrorCode.None, farmingItemList);
    }

    ErrorCode IsValidStageReward(Int32 playerStage, List<FarmingItem> farmingItemList)
    {
        var stageItemList = MasterDataDb.s_stageItem
                            .FindAll(stageItem => stageItem.StageCode == playerStage);
        if (stageItemList is null)
        {
            return ErrorCode.InvalidFarmingItem;
        }

        foreach (var stageItem in stageItemList)
        {
            var result = farmingItemList.Find(item => item.ItemCode == stageItem.ItemCode
                                            && item.Count <= stageItem.Count);
            if (result is null)
            {
                return ErrorCode.FarmingItemNotMatchStageItem;
            }
        }

        return ErrorCode.None;
    }

    bool IsMoney(Int32 itemCode)
    {
        var baseItem = MasterDataDb.s_baseItem.Find(item => item.Code == itemCode);
        if (baseItem is null)
        {
            return false;
        }

        if (baseItem.Attribute == (int)ItemAttribute.Money)
        {
            return true;
        }

        return false;
    }

    async Task<List<int>?> SaveItemList(Int32 playerId, List<FarmingItem> farimingItemList)
    {
        List<int> itemIdList = new();

        for (int i = 0; i<farimingItemList.Count; i++)
        {
            if (IsMoney(farimingItemList[i].ItemCode) == false)
            {
                var(saveItemErrorCode, itemId)
                    = await SaveItem(playerId, farimingItemList[i]);
                
                itemIdList.Add(itemId);
                if (saveItemErrorCode != ErrorCode.None )
                {
                    await RollbackItems(itemIdList);
                    return null;
                }
            }
        }

        return itemIdList;
    }

    async Task<Tuple<ErrorCode, Int32>> SaveItem(Int32 playerId, FarmingItem farmingItem)
    {
        Item? item = Item.InitItem(playerId, farmingItem.ItemCode, farmingItem.Count);
        
        return await _itemDb.AddItemToPlayerItemList(playerId, item);
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


    async Task<ErrorCode> SaveMoneyAndExp(Int32 playerId, List<FarmingItem> farimingItemList, List<KillNPC> killNPCList)
    {
        var (saveMoneyError, moneyAmount) = await SaveMoney(playerId, farimingItemList);
        if (saveMoneyError != ErrorCode.None )
        {
            return saveMoneyError;
        }

        var saveExpErrorCode = await SaveExp(playerId, killNPCList);
        if (saveExpErrorCode != ErrorCode.None )
        {
            await RollbackSavedMoney(playerId, moneyAmount);
            return saveExpErrorCode;
        }

        return ErrorCode.None;
    }
    async Task<ErrorCode> SaveExp(Int32 playerId, List<KillNPC> killNPCList)
    {
        int totalExp = SumExp(killNPCList);

        var addExpErrorCode = await _playerDb.AddExp(playerId, totalExp);
        if (addExpErrorCode != ErrorCode.None )
        {
            return addExpErrorCode;
        }

        return ErrorCode.None;
    }

    int SumExp(List<KillNPC> killNPCList)
    {
        int totalExp = 0;
        foreach (var killNPC in killNPCList)
        {
            var NPC = MasterDataDb.s_stageAttackNPC.Find(baseNPC => baseNPC.NPCCode == killNPC.NPCCode);
            totalExp += NPC.Exp * killNPC.Count;
        }

        return totalExp;
    }

    async Task<Tuple<ErrorCode, int>> SaveMoney(Int32 playerId, List<FarmingItem> farimingItemList)
    {
        int moneyIndex = FindMoneyIndex(farimingItemList);
        if (moneyIndex == -1)
        {
            return new (ErrorCode.None, 0);
        }

        var addMoneyErrorCode = await _playerDb.AddMoney(playerId, farimingItemList[moneyIndex].Count);
        if (addMoneyErrorCode != ErrorCode.None)
        {
            return new(addMoneyErrorCode, 0);
        }

        return new(ErrorCode.None, farimingItemList[moneyIndex].Count);
    }

    int FindMoneyIndex(List<FarmingItem> farimingItemList)
    {
        for (int i = 0; i < farimingItemList.Count; i++)
        {
            if (IsMoney(farimingItemList[i].ItemCode))
            {
                return i;
            }
        }
        return -1;
    }

    async Task RollbackSavedMoney(Int32 playerId, Int32 moneyAmount)
    {
        var savaMoneyErrorCode = await _playerDb.AddMoney(playerId, moneyAmount * -1);
        if (savaMoneyErrorCode != ErrorCode.None)
        {
            // TODO : rollback Error log
        }
    }
}


