using System;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class EnhanceItemController : ControllerBase
{
    readonly ILogger<EnhanceItemController> _logger;
    readonly IItemEnhanceDb _itemEnhanceDb;

    public EnhanceItemController(ILogger<EnhanceItemController> logger,
        IItemEnhanceDb itemEnhanceDb)
	{
        _logger = logger;
        _itemEnhanceDb = itemEnhanceDb;
	}

    [HttpPost]
    public async Task<EnhanceItemRes> EnhanceItemThenGetResultItem(EnhanceItemReq request)
    {
        EnhanceItemRes response = new EnhanceItemRes();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        var (EnhancePlayerItemErrorCode, resultItem) =
            await _itemEnhanceDb.EnhancePlayerItem(request.ItemId, playerId);
        response.Result = EnhancePlayerItemErrorCode;
        response.ResultItem = resultItem;
        return response;
    }
}

