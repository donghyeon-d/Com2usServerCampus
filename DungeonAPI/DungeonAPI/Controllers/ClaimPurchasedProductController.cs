using System;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimPurchasedProductController : ControllerBase
{
    readonly ILogger<ClaimPurchasedProductController> _logger;
    readonly IInAppPurchaseDb _inAppPurchaseDb;

    public ClaimPurchasedProductController(ILogger<ClaimPurchasedProductController> logger, IInAppPurchaseDb inAppPurchaseDb)
	{
        _logger = logger;
        _inAppPurchaseDb = inAppPurchaseDb;
	}

    [HttpPost]
    public async Task<ClaimPurchasedProductRes> ClaimPurchasedProductToMailBox(ClaimPurchasedProductReq request)
    {
        ClaimPurchasedProductRes response = new ClaimPurchasedProductRes();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        response.Result = 
            await _inAppPurchaseDb.ProvidePurchasedProductToMail(playerId, request.ReceiptId);

        return response;
    }
}

