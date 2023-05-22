using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using DungeonAPI.Util;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;
using DungeonAPI.Enum;
using ZLogger;

namespace DungeonAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FarmingItemController : ControllerBase
    {
        readonly ILogger<FarmingItemController> _logger;
        readonly IMemoryDb _memoryDb;

        public FarmingItemController(ILogger<FarmingItemController> logger,
            IMemoryDb memoryDb)
        {
            _logger = logger;
            _memoryDb = memoryDb;
        }

        [HttpPost]
        public async Task<FarmingItemRes> FarmingItemInStage(FarmingItemReq request)
        {
            PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

            FarmingItemRes response = new();

            var LogicErrorCode = await FarmingItemLogic(player, request);

            response.Result = LogicErrorCode;
            _logger.ZLogInformationWithPayload(new { PlayerId = player.Id, ItemCode = request.ItemCode }, response.Result.ToString());

            return response;
        }

        async Task<ErrorCode> FarmingItemLogic(PlayerInfo player, FarmingItemReq request)
        {
            var checkValidRequestErrorCode = CheckVaildRequest(player, request);
            if (checkValidRequestErrorCode != ErrorCode.None)
            {
                await SetExitDungeon(request.Email);
                return checkValidRequestErrorCode;
            }

            var (getDungeonInfoErrorCode, dungeonInfo) = await GetDungeonInfo(request.Email);
            if (getDungeonInfoErrorCode != ErrorCode.None || dungeonInfo is null)
            {
                return getDungeonInfoErrorCode;
            }

            var addFarmingItemErrorCode = await AddFarmingItem(request.Email, dungeonInfo, request.ItemCode);
            if (addFarmingItemErrorCode == ErrorCode.None)
            {
                return addFarmingItemErrorCode;
            }

            return ErrorCode.None;
        }

        async Task<ErrorCode> AddFarmingItem(string email, InDungeon dungeonInfo, Int32 itemCode)
        {
            var addFarmingItemCountErrorCode = AddFarmingItemCount(dungeonInfo, itemCode);
            if (addFarmingItemCountErrorCode != ErrorCode.None)
            {
                return addFarmingItemCountErrorCode;
            }

            var setFarmingItemErrorCode = await _memoryDb.SetDungeonInfo(email, dungeonInfo);
            if (setFarmingItemErrorCode == ErrorCode.None)
            {
                RollbackAddedItemCount(dungeonInfo, itemCode);
                return setFarmingItemErrorCode;
            }

            return ErrorCode.None;
        }

        void RollbackAddedItemCount(InDungeon dungeonInfo, Int32 itemCode)
        {
            var index = dungeonInfo.ItemList.FindIndex(item => item.ItemCode == itemCode);
            dungeonInfo.ItemList[index].Count--;
        }

        ErrorCode AddFarmingItemCount(InDungeon dungeonInfo, Int32 itemCode)
        {
            var index = dungeonInfo.ItemList.FindIndex(item => item.ItemCode == itemCode);

            if (IsMaxItemCount(dungeonInfo.ItemList[index]))
            {
                return ErrorCode.TooMuchItem;
            }
            else
            {
                AddItem(dungeonInfo.ItemList[index]);
                return ErrorCode.None;
            }
        }

        bool IsMaxItemCount(FarmingItem item)
        {
            if (item.Count == item.Max)
            {
                return true;
            }
            return false;
        }

        void AddItem(FarmingItem farmingItem)
        {
            farmingItem.Count++;
        }

        ErrorCode CheckVaildRequest(PlayerInfo player, FarmingItemReq request)
        {
            if (IsPlayerInDungeon(player.Status) == false)
            {
                return ErrorCode.InvalidPlayerStatusNotPlayDungeon;
            }

            if (IsVaildFarmingItem(player.CurrentStage, request.ItemCode) == false)
            {
                return ErrorCode.InvalidFarmingItem;
            }

            return ErrorCode.None;
        }

        bool IsPlayerInDungeon(string playerStatus)
        {
            if (playerStatus != PlayerStatus.DungeonPlay.ToString())
            {
                return false;
            }
            return true;
        }

        bool IsVaildFarmingItem(Int32 playerStage, Int32 itemCode)
        {
            var stageItems = MasterDataDb.s_stageItem.FindAll(item => item.StageCode == playerStage);
            if (stageItems is null || stageItems.Count == 0)
            {
                return false;
            }

            foreach (var stageItem in stageItems)
            {
                if (stageItem.ItemCode == itemCode)
                {
                    return true;
                }
            }
            return false;
        }

        async Task<Tuple<ErrorCode, InDungeon?>> GetDungeonInfo(string email)
        {
            var (getDungeonInfoErrorCode, dungeonInfo) = await _memoryDb.GetDungeonInfo(email);
            if (getDungeonInfoErrorCode != ErrorCode.None || dungeonInfo is null)
            {
                return new(getDungeonInfoErrorCode, null);
            }

            return new(ErrorCode.None, dungeonInfo);
        }

        async Task SetExitDungeon(string email)
        {
            var changeUserStatusErrorCode
                = await _memoryDb.ChangeUserStatus(email, PlayerStatus.LogIn);
            if (changeUserStatusErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(new { Email = email }, "RollBackError " + changeUserStatusErrorCode.ToString());
            }

            var deleteDungeonInfoErrorCode = await _memoryDb.DeleteDungeonInfo(email);
            if (deleteDungeonInfoErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(new { Email = email }, "RollBackError " + changeUserStatusErrorCode.ToString());
            }
        }
    }
}
