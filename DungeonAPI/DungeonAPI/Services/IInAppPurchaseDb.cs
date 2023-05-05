using System;
namespace DungeonAPI.Services;

public interface IInAppPurchaseDb
{
	public Task<ErrorCode> ProvidePurchasedProductToMail(Int32 playerId, String reciptId);
}

