using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;

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
            FarmingItemRes response = new();

            string playerStatus = HttpContext.Items["PlayerStatus"].ToString();
            Int32 playerStage = int.Parse(HttpContext.Items["PlayerStage"].ToString());

            var checkValidRequestErrorCode = CheckVaildRequest(playerStatus, playerStage, request.FarmingItem);
            if (checkValidRequestErrorCode != ErrorCode.None)
            {
                response.Result = checkValidRequestErrorCode;
                await SetExitDungeon(request.Email);
                return response;
            }

            var (getFarmingItemErrorCode, farmingItemList)
                = await _memoryDb.GetFarmingItemList(request.Email);
            if (getFarmingItemErrorCode != ErrorCode.None || farmingItemList is null)
            {
                response.Result = getFarmingItemErrorCode;
                return response;
            }

            AddFarmingItemToList(farmingItemList, request.FarmingItem);

            var setFarmingItemErrorCode = await _memoryDb.SetFarmingItemList(request.Email, farmingItemList);
            if (setFarmingItemErrorCode == ErrorCode.None)
            {
                response.Result = setFarmingItemErrorCode;
                return response;
            }

            return response;
        }

        ErrorCode CheckVaildRequest(string playerStatus, Int32 playerStage, FarmingItem farmingItem)
        {
            if (IsPlayerInDungeon(playerStatus) == false)
            {
                return ErrorCode.InvalidPlayerStatusNotPlayDungeon;
            }

            if (IsVaildFarmingItem(playerStage, farmingItem) == false)
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

        bool IsVaildFarmingItem(Int32 playerStage, FarmingItem farmingItem)
        {
            var stageItems = MasterDataDb.s_stageItem.FindAll(item => item.StageCode == playerStage);
            if (stageItems is null || stageItems.Count == 0)
            {
                return false;
            }

            foreach( var stageItem in stageItems )
            {
                if ( stageItem.ItemCode == farmingItem.ItemCode)
                {
                    return true;
                }
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

        // 보상 받을 때 아이템 db에 넣을때 정리 한번 해줘야 함
        void AddFarmingItemToList(List<FarmingItem> list, FarmingItem farmingItem)
        {
            int index = list.FindIndex(item => item.ItemCode == farmingItem.ItemCode);

            list.Add(farmingItem);
        }
    }
}
