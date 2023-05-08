using System;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;

namespace DungeonAPI.Services;

public class InAppPurchaseDb : GameDb, IInAppPurchaseDb
{
    readonly ILogger<InAppPurchaseDb> _logger;
    readonly IMailDb _mailDb;

	public InAppPurchaseDb(ILogger<InAppPurchaseDb> logger, IOptions<DbConfig> dbConfig,
        IMailDb mailDb)
        : base(logger, dbConfig)
	{
        _logger = logger;
        _mailDb = mailDb;
	}

    public async Task<ErrorCode> ProvidePurchasedProductToMail(Int32 playerId, String receiptId)
    {
        var (CheckReceiptErrorCode, productId) = CheckReceiptThenGetProductCode(receiptId);
        if (CheckReceiptErrorCode != ErrorCode.None)
        {
            return CheckReceiptErrorCode;
        }

        var CheckDuplicatedReceiptError = await CheckDuplicatedReceipt(receiptId);
        if (CheckDuplicatedReceiptError != ErrorCode.None)
        {
            return CheckDuplicatedReceiptError;
        }

        var InsertPurchaseInfoError = await InsertPurchaseInfoToList(playerId, receiptId, productId);
        if (InsertPurchaseInfoError != ErrorCode.None)
        {
            return InsertPurchaseInfoError;
        }

        ErrorCode SendToMailErrorCode = await SendToMail(playerId, productId);
        if (SendToMailErrorCode != ErrorCode.None)
        {
            await DeletePurchaseInfoWhenFail(receiptId);
            return SendToMailErrorCode;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> CheckDuplicatedReceipt(String receiptId)
    {
        try
        {
            var registeredReceipts = await _queryFactory.Query("InAppPurchase")
                            .Where("ReceiptId", receiptId)
                            .FirstOrDefaultAsync<InAppPurchase>();
            if (registeredReceipts is not null)
            {
                return ErrorCode.DuplicatedReceipt;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.CheckDuplicatedReceiptFailException;
        }
    }

    async Task<ErrorCode> InsertPurchaseInfoToList(Int32 playerId, String receiptId, Int32 productId)
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
                                ProductCode = productId
                            });
            if (result != 1)
            {
                return ErrorCode.InsertInAppPurchaseFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.InsertInAppPurchaseFailException;
        }
    }

    async Task<ErrorCode> DeletePurchaseInfoWhenFail(String receiptId)
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
            return ErrorCode.DeletePurchaseInfoFailException;
        }
    }

    async Task<ErrorCode> SendToMail(Int32 playerId, Int32 productId)
    {
        Mail mail = new Mail
        {
            PlayerId = playerId,
            Title = "Provide Purchased InAppProduct",
            PostDate = DateTime.Now,
            ExpiredDate = DateTime.MaxValue,
            IsOpened = 0,
            IsReceivedReward = 0,
            IsDeleted = 0,
            CanDelete = 0,
            Sender = "InAppPurchase"
        };

        MailContent mailContent = new MailContent
        {
            Content = $"Thank you for Purchasing Product. " +
            $"You can get Product in Mailbox!"
        };

        List<MailReward> mailRewards = new List<MailReward>();
        var products = MasterDataDb.s_inAppProduct
                    .FindAll(product => product.Code == productId);
        foreach (MasterData.InAppProduct product in products)
        {
            mailRewards.Add(new MailReward
            {
                BaseItemCode = product.ItemCode,
                ItemCount = product.ItemCount
            });
        }

        try
        {
            var (createMailErrorCode, mailId) = await _mailDb.CreateMail(mail, mailContent, mailRewards);
            if (createMailErrorCode != ErrorCode.None)
            {
                return createMailErrorCode;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.SendToMailExceptionAtProvidePurchasedProduct;
        }
    }

    // 임시버전. 영수증 확인해서 구매물품 코드번호 주기
    Tuple<ErrorCode, Int32> CheckReceiptThenGetProductCode(String receiptId)
    {
        // 영수증 검사
        if (string.IsNullOrEmpty(receiptId))
        {
            return new Tuple<ErrorCode, Int32>(ErrorCode.WrongReceipt, -1);
        }
        else if (char.IsDigit(receiptId[0]))
        {
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, 1);
        }
        else if (char.IsLower(receiptId[0]))
        {
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, 2);
        }
        else
        {
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, 3);
        }
    }
}

