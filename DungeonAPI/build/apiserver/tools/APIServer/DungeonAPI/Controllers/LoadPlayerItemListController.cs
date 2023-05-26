using Microsoft.AspNetCore.Mvc;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using ZLogger;
using DungeonAPI.ModelDB;

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

        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        var (loadAllItemsErrorCode, itemList) = await _gameDb.LoadPlayerItemListAsync(player.Id);
        if (loadAllItemsErrorCode != ErrorCode.None)
        {
            response.Result = loadAllItemsErrorCode;
            _logger.ZLogInformationWithPayload(new { Player = player.Id }, response.Result.ToString());
            return response;
        }

        response.ItemList = itemList;
        _logger.ZLogInformationWithPayload(new { Player = player.Id, ItemCount = response.ItemList.Count() }, response.Result.ToString());
        return response;
    }
}
