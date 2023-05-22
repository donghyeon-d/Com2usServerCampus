using Microsoft.AspNetCore.Mvc;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoadPlayerItemListController : ControllerBase
{
    readonly ILogger<LoadPlayerItemListController> _logger;
    readonly IGameDb _gameDb;

    public LoadPlayerItemListController(ILogger<LoadPlayerItemListController> logger,
    IGameDb gameDb)
    {
        _logger = logger;
        _gameDb = gameDb;
    }

    [HttpPost]
    public async Task<PlayerItemListRes> LoadPlayerItemList(PlayerItemListReq request)
    {
        PlayerItemListRes response = new ();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        var (loadAllItemsErrorCode, itemList) = await _gameDb.LoadPlayerItemListAsync(playerId);
        if (loadAllItemsErrorCode != ErrorCode.None)
        {
            response.Result = loadAllItemsErrorCode;
            _logger.ZLogInformationWithPayload(new { Player = playerId }, response.Result.ToString());
            return response;
        }

        response.ItemList = itemList;
        _logger.ZLogInformationWithPayload(new { Player = playerId, ItemCount = response.ItemList.Count() }, response.Result.ToString());
        return response;
    }
}
