using System;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using MySqlConnector;
using ZLogger;

namespace DungeonAPI.Services;

public partial class GameDb : IGameDb
{
    public async Task<ErrorCode> RegistReceipt(Int32 playerId, String receiptId, Int32 productCode)
    {
        try
        {
            int result = await _queryFactory
                            .Query("InAppPurchase")
                            .InsertAsync(new
                            {
                                PlayerId = playerId,
                                ReceiptId = receiptId,
                                ReceiveDate = DateTime.Today,
                                ProductCode = productCode
                            });
            if (result != 1)
            {
                return ErrorCode.InsertInAppPurchaseFail;
            }
            return ErrorCode.None;
        }
        catch (MySqlException e)
        {
            if (e.Number == 1062)
            {
                return ErrorCode.DuplicatedReceipt;
            }
            _logger.ZLogWarning(e.Message);
            return ErrorCode.InsertInAppPurchaseFailException;
        }
    }

    public async Task<ErrorCode> DeleteReceipt(String receiptId)
    {
        try
        {
            int result = await _queryFactory
                        .Query("InAppPurchase")
                        .Where("ReceiptId", receiptId)
                        .DeleteAsync();
            if (result != 1)
            {
                return ErrorCode.DeletePurchaseInfoFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.DeletePurchaseInfoFailException;
        }
    }
}