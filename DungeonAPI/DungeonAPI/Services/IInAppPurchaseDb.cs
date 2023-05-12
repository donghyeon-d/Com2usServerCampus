using System;
namespace DungeonAPI.Services;

public interface IInAppPurchaseDb
{
    public Task<ErrorCode> RegistReceipt(Int32 playerId, String receiptId, Int32 productCode);
    public Task<ErrorCode> DeleteReceipt(String receiptId);
}

