using DungeonAPI.Services;
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

        var (saveReward, farmingItemList) = await SaveReward(request.Email, playerId, playerStage);
        if (saveReward != ErrorCode.None)
        {
            return new StageCompleteRes() { Result = saveReward };
        }

        var changePlayerStatusErrorCode = await ChangePlayerStatusToLogin(request.Email);
        if (changePlayerStatusErrorCode != ErrorCode.None)
        {
            await SetLogOff(request.Email);
            return new StageCompleteRes() { Result = changePlayerStatusErrorCode };
        }

        return new StageCompleteRes() { Result = ErrorCode.None, RewardList = farmingItemList };
    }

    async Task<ErrorCode> ChangePlayerStatusToLogin(string email)
    {
        var changeUserStatusErrorCode = await _memoryDb.ChangeUserStatus(email, PlayerStatus.LogIn);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            return changeUserStatusErrorCode;
        }

        var deleteDungeonInfoErrorCode = await _memoryDb.DeleteDungeonInfo(email);
        if (deleteDungeonInfoErrorCode != ErrorCode.None)
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
        var (sumExpErrorCode, totalExp) = await SumExpAboutKillNPCs(email, stage);
        if(sumExpErrorCode != ErrorCode.None)
        {
            return new(sumExpErrorCode, null);
        }

        var (totalFarmingItemListErrorCode, totalFarmingList) = await TotalFarmingItemList(email);
        if (totalFarmingItemListErrorCode == ErrorCode.None || totalFarmingList is null)
        {
            return new(totalFarmingItemListErrorCode, null);
        }

        var (saveItemErrorCode, itemIdList) = await SaveItemToInventory(playerId, totalFarmingList);
        if (saveItemErrorCode != ErrorCode.None || itemIdList is null)
        {
            return new(saveItemErrorCode, null);
        }

        var saveMoneyAndExpErrorCode = await SaveMoneyAndExpToPlayerData(playerId, totalExp, totalFarmingList);
        if (saveMoneyAndExpErrorCode != ErrorCode.None)
        {
            await RollbackItems(itemIdList);
            return new(saveMoneyAndExpErrorCode, null);
        }

        return new(ErrorCode.None, totalFarmingList);
    }

    async Task<Tuple<ErrorCode, int>> SumExpAboutKillNPCs(string email, Int32 stage)
    {
        var (getKillNPCListErrorCode, NPCList) = await _memoryDb.GetKillNPCList(email);
        if (getKillNPCListErrorCode != ErrorCode.None || NPCList is null)
        {
            return new(getKillNPCListErrorCode, -1);
        }

        int totalExp = SumNPCsExp(NPCList, stage);

        return new (ErrorCode.None, totalExp);
    }

    int SumNPCsExp(List<KillNPC> NPCList, Int32 stage)
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

    async Task<Tuple<ErrorCode, List<FarmingItem>?>> TotalFarmingItemList(string email)
    {
        return await _memoryDb.GetFarmingItemList(email);
    }

    async Task<Tuple<ErrorCode, List<int>?>> SaveItemToInventory(Int32 playerId, List<FarmingItem> farmingList)
    {
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

    async Task<ErrorCode> SaveMoneyAndExpToPlayerData(Int32 playerId, int totalExp, List<FarmingItem> totalFarmingList)
    {
        FarmingItem money = totalFarmingList.Find(item => Util.ItemAttribute.IsGold(item.ItemCode));

        var addMoneyErrorCode =  await _playerDb.AddMoney(playerId, money.Count);
        if (addMoneyErrorCode != ErrorCode.None)
        {
            return addMoneyErrorCode;
        }

        var addExpErrorCode = await _playerDb.AddExp(playerId, totalExp);
        if(addExpErrorCode != ErrorCode.None)
        {
            await RollbackAddMoney(playerId, money.Count);
            return addExpErrorCode;
        }

        return ErrorCode.None;
    }

    async Task RollbackAddMoney(Int32 playerId, int amount)
    {
        await _playerDb.AddMoney(playerId, amount * -1);
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


