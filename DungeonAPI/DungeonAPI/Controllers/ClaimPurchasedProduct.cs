using System;
using DungeonAPI.MessageBody;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimPurchasedProduct : ControllerBase
{
    readonly ILogger<ClaimPurchasedProduct> _logger;
    readonly IInAppPurchaseDb _inAppPurchaseDb;

    public ClaimPurchasedProduct(ILogger<ClaimPurchasedProduct> logger, IInAppPurchaseDb inAppPurchaseDb)
	{
        _logger = logger;
        _inAppPurchaseDb = inAppPurchaseDb;
	}

    [HttpPost]
    public async Task<ClaimPurchasedProductRes> ClaimPurchasedProductToMailBox(ClaimPurchasedProductReq request)
    {
        ClaimPurchasedProductRes response = new ClaimPurchasedProductRes();

        var playerIdValue = HttpContext.Items["PlayerId"];
        Int32 playerId = int.Parse(playerIdValue.ToString());

        response.Result = 
            await _inAppPurchaseDb.ProvidePurchasedProductToMail(playerId, request.ReceiptId);

        return response;
    }
}

