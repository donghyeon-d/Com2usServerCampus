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
                return response;
            }

            var (getFarmingItemErrorCode, itemList)
                = await _memoryDb.GetFarmingItemList(request.Email);
            if (getFarmingItemErrorCode != ErrorCode.None || itemList is null)
            {
                response.Result = getFarmingItemErrorCode;
                return response;
            }

            AddFarmingItemToList(itemList, request.FarmingItem);

            var setFarmingItemErrorCode = await _memoryDb.SetFarmingItemList(request.Email, itemList);
            if (setFarmingItemErrorCode == ErrorCode.None)
            {
                response.Result = setFarmingItemErrorCode;
                return response;
            }

            return response;
        }

        void AddFarmingItemToList(List<FarmingItem> list, FarmingItem farmingItem)
        {
            int index = list.FindIndex(item => item.ItemCode == farmingItem.ItemCode);

            if (index == -1)
            {
                list.Add(farmingItem);
            }
            else
            {
                list[index].Count++;
            }
        }

        ErrorCode CheckVaildRequest(string playerStatus, Int32 playerStage, FarmingItem farmingItem)
        {
            if (playerStatus != PlayerStatus.DungeonPlay.ToString())
            {
                return ErrorCode.InvalidPlayerStatusNotPlayStage;
            }

            if (IsVaildFarmingItem(playerStage, farmingItem) == false)
            {
                return ErrorCode.InvalidFarmingItem;
            }

            return ErrorCode.None;
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
    }
}
