using System;
namespace DungeonAPI.Services;

public interface IInAppPurchaseDb
{
	public Task<ErrorCode> ProvidePurchasedProductToMail(Int32 playerId, String reciptId);
    public Task<ErrorCode> RegistReceipt(Int32 playerId, String receiptId, Int32 productCode);
}

