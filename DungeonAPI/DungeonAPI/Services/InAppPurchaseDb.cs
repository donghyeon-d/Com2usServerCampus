using System;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using MySqlConnector;

namespace DungeonAPI.Services;

public class InAppPurchaseDb : GameDb, IInAppPurchaseDb
{
    readonly ILogger<InAppPurchaseDb> _logger;

    public InAppPurchaseDb(ILogger<InAppPurchaseDb> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
    {
        _logger = logger;
    }

    public async Task<ErrorCode> ProvidePurchasedProductToMail(Int32 playerId, String receiptId)
    {
        var (CheckReceiptErrorCode, productId) = ValidCheckReceiptThenGetProductCode(receiptId);
        if (CheckReceiptErrorCode != ErrorCode.None)
        {
            return CheckReceiptErrorCode;
        }

        var CheckDuplicatedReceiptError = await CheckDuplicatedReceipt(receiptId);
        if (CheckDuplicatedReceiptError != ErrorCode.None)
        {
            return CheckDuplicatedReceiptError;
        }

        var InsertPurchaseInfoError = await InsertPurchaseInfoToManageList(playerId, receiptId, productId);
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
        Mail mail = MakeMail(playerId);

        MailContent mailContent
            = MakeMailContent("Thank you for Purchasing Product. " +
                                "You can get Product in Mailbox!");

        List<MailReward> mailRewards = MakeMailRewardList(productId);

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

    Mail MakeMail(Int32 playerId)
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
        return mail;
    }

    MailContent MakeMailContent(string content)
    {
        MailContent mailContent = new MailContent
        {
            Content = content
        };
        return mailContent;
    }

    List<MailReward> MakeMailRewardList(Int32 productId)
    {
        List<MailReward> mailRewards = new List<MailReward>();

        var products = MasterDataDb.s_inAppProduct
                    .FindAll(product => product.Code == productId);

        foreach (MasterData.InAppProduct product in products)
        {
            mailRewards.Add(new MailReward
            {
                BaseItemCode = product.ItemCode,
                ItemCount = product.ItemCount,
                IsReceived = false
            });
        }

        return mailRewards;
    }


}

