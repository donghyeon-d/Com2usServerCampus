using System;
using ZLogger;
using DungeonAPI.MessageBody;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class EnhanceItemController
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

        var (EnhancePlayerItemErrorCode, resultItem) =
            await _itemEnhanceDb.EnhancePlayerItem(request.ItemId);
        response.Result = EnhancePlayerItemErrorCode;
        response.ResultItem = resultItem;
        return response;
    }
}

