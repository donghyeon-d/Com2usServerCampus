using DungeonAPI.Services;
using DungeonAPI.RequestResponse;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;
using static Humanizer.In;
using DungeonAPI.Enum;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StageCompleteController : ControllerBase
{
    readonly ILogger<StageCompleteController> _logger;
    readonly IMemoryDb _memoryDb;
    readonly IGameDb _gameDb;

    public StageCompleteController(ILogger<StageCompleteController> logger,
    IMemoryDb memoryDb, IGameDb gameDb)
    {
        _logger = logger;
        _memoryDb = memoryDb;
        _gameDb = gameDb;
    }

    [HttpPost]
    public async Task<StageCompleteRes> ProcessRequest(StageCompleteReq request)
    {
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        StageCompleteRes response = await StageCompleteThenApplyResult(request, player);

        _logger.ZLogInformationWithPayload(new { PlayerId = player.Id, Stage = request.Stage }, response.Result.ToString());

        return new StageCompleteRes() {  };
    }

    async Task<StageCompleteRes> StageCompleteThenApplyResult(StageCompleteReq request, PlayerInfo player)
    {
        StageCompleteRes response = new() { Result = ErrorCode.None};

        if (IsValidRequest(player.Status, player.CurrentStage, request.Stage) == false)
        {
            await SetExitDungeon(player.Id);
            response.Result = ErrorCode.StageCompleteInvalidPlayerStatus;
            return response;
        }

        var saveCompletedDungeonErrorCode = await SaveCompletedDungeon(player.Id, player.CurrentStage);
        if (saveCompletedDungeonErrorCode != ErrorCode.None)
        {
            response.Result = saveCompletedDungeonErrorCode;
            return response;
        }

        var (saveRewardErrorCode, farmingItemList) = await SaveReward(player.Id, player.CurrentStage);
        if (saveRewardErrorCode != ErrorCode.None)
        {
            response.Result = saveRewardErrorCode;
            return response;
        }

        var changePlayerStatusErrorCode = await ChangePlayerStatusToLogin(player.Id);
        if (changePlayerStatusErrorCode != ErrorCode.None)
        {
            await SetLogOff(player.Id);
            response.Result = changePlayerStatusErrorCode;
            return response;
        }

        response.RewardList = farmingItemList;
        return response;
    }

     async Task<ErrorCode> SaveCompletedDungeon(Int32 playerId, Int32 playerStage)
    {
        var addCompletedDungeonErrorCoed =  await _gameDb.AddCompletedDungeon(playerId, playerStage);
        if (addCompletedDungeonErrorCoed != ErrorCode.None)
        {
            return addCompletedDungeonErrorCoed;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> ChangePlayerStatusToLogin(Int32 playerId)
    {
        var changeUserStatusErrorCode = await _memoryDb.ChangeUserStatus(playerId, PlayerStatus.LogIn);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            return changeUserStatusErrorCode;
        }

        var deleteDungeonInfoErrorCode = await _memoryDb.DeleteDungeonInfo(playerId);
        if (deleteDungeonInfoErrorCode != ErrorCode.None
            && deleteDungeonInfoErrorCode != ErrorCode.DeleteFarmingItemListNotExist)
        {
            return deleteDungeonInfoErrorCode;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> SetLogOff(Int32 playerId)
    {
        var changeUserStatusErrorCode = await _memoryDb.ChangeUserStatus(playerId, PlayerStatus.LogOff);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, "RollBackError " + changeUserStatusErrorCode.ToString());
            return changeUserStatusErrorCode;
        }

        return ErrorCode.None;
    }

    async Task<Tuple<ErrorCode, List<FarmingItem>?>> SaveReward(Int32 playerId, Int32 stage)
    {
        var (getDungeonInfoErrorCode, dungeonInfo) = await _memoryDb.GetDungeonInfo(playerId);
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
                await _gameDb.AddItemToPlayerItemList(playerId, item);
            if (addItemErrorCode != ErrorCode.None)
            {
                var rollbackErrorCode = await RollbackAddedItem(itemIdList);
                if (rollbackErrorCode != ErrorCode.None)
                {
                    _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, "RollBackError " + rollbackErrorCode.ToString());
                }
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
            // IsValidRequest() 에서 이미 확인하기에 item이 null일 수 없음
            if (Util.ItemAttribute.IsGold(item.ItemCode) == false)
            {
                itemList.Add(item);
            }
        }

        return itemList;
    }

    async Task<ErrorCode> RollbackAddedItem(List<int> itemIdList)
    {
        foreach (var itemId in itemIdList)
        {
            var rollbackErrorCode = await _gameDb.DeleteItem(itemId);
            if (rollbackErrorCode != ErrorCode.None)
            {
                return rollbackErrorCode;
            }
        }

        return ErrorCode.None;
    }

    int SumKillNPCListExp(List<KillNPC> NPCList, Int32 stage)
    {
        int totalExp = 0;

        foreach(var killNPC in NPCList)
        {
            // IsValidRequest()에서 NPC유효성 검사 이미 했기 때문에 findNPC가 null일 수 없음
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

        var addExpErrorCode = await _gameDb.AddExp(playerId, totalExp);
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

        //IsEmptyItemList()에서 확인했기 때문에 farmingList가 null일 수 없음
        FarmingItem? money = farmingList.Find(item => Util.ItemAttribute.IsGold(item.ItemCode));
        if (money is null || money.Count == 0)
        {
            return new(ErrorCode.None, 0);
        }

        var addMoneyErrorCode = await _gameDb.AddMoney(playerId, money.Count);
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

        var addMoneyErrorCode =  await _gameDb.AddMoney(playerId, amount * -1);
        if (addMoneyErrorCode != ErrorCode.None)
        {
            _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, "RollBackError " + addMoneyErrorCode.ToString());
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

    async Task SetExitDungeon(Int32 playerId)
    {
        var changeUserStatusErrorCode
            = await _memoryDb.ChangeUserStatus(playerId, PlayerStatus.LogIn);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, "RollBackError " + changeUserStatusErrorCode.ToString());
        }

        var deleteDungeonInfoErrorCode = await _memoryDb.DeleteDungeonInfo(playerId);
        if (deleteDungeonInfoErrorCode != ErrorCode.None)
        {
            _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, "RollBackError " + deleteDungeonInfoErrorCode.ToString());
        }
    }

    async Task RollbackItems(List<int> itemIdList)
    {
        foreach (var itemId in itemIdList) 
        {
            var deleteItemErrorCode = await _gameDb.DeleteItem(itemId);
            if (deleteItemErrorCode != ErrorCode.None )
            {
                _logger.ZLogErrorWithPayload(new { iItemId = itemId }, "RollBackError " + deleteItemErrorCode.ToString());
            }
        }
    }
}


