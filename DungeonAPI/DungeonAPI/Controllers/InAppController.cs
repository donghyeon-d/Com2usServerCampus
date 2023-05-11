using System;
using DungeonAPI.ModelDB;
using DungeonAPI.ModelsDB;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class InAppController : ControllerBase
{
    readonly ILogger<InAppController> _logger;
    readonly IInAppPurchaseDb _inAppPurchaseDb;
    readonly IMailDb _mailDb;

    public InAppController(ILogger<InAppController> logger, IInAppPurchaseDb inAppPurchaseDb,
        IMailDb mailDb)
	{
        _logger = logger;
        _inAppPurchaseDb = inAppPurchaseDb;
        _mailDb = mailDb;
    }

    [HttpPost]
    public async Task<InAppRes> ClaimPurchasedProductToMailBox(InAppReq request)
    {
        InAppRes response = new InAppRes();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        response.Result = 
            await _inAppPurchaseDb.ProvidePurchasedProductToMail(playerId, request.ReceiptId);

        return response;
    }

    async Task<ErrorCode> ProvidePurchasedProductToMail(Int32 playerId, String receiptId)
    {
        var (CheckReceiptErrorCode, productId) = ValidCheckReceiptThenGetProductCode(receiptId);
        if (CheckReceiptErrorCode != ErrorCode.None)
        {
            return CheckReceiptErrorCode;
        }

        var InsertPurchaseInfoError = await _inAppPurchaseDb.RegistReceipt(playerId, receiptId, productId);
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

    Tuple<ErrorCode, Int32> ValidCheckReceiptThenGetProductCode(String receiptId)
    {
        // 영수증 검사 필요. (임시)영수증에 따른 물품 번호
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

    async Task<ErrorCode> SendItemsToMail(Int32 playerId, Int32 productId)
    {
        // 메일 리스트 만들기
        List<Mail> mailList = InitInAppMailList(playerId, productId);
        // foreach _mailDb.SendMail
           // 잘못되면
           // 죽이고
           // break
           // List<int>mailId add
           // 

    }

    List<Mail>? InitInAppMailList(Int32 playerId, Int32 productId)
    {
        var items = MasterDataDb.s_inAppProduct.FindAll(product => product.Code == productId);
        if (items is null || items.Count == 0)
        {
            return null;
        }

        List<Mail> mailList = new();
        foreach (var item in items)
        {
            Mail mail = new Mail()
            {
                PlayerId = playerId,
                Title = "InAppProduct",
                Content = "Thank you for Purchasing Product. You can get Product in Mailbox"
                ExpiredDate = DateTime.MaxValue,
                CanDelete = true,
                Sender = "InApp",
                ItemCode = item.ItemCode,
                ItemCount = item.ItemCount
            };
            mailList.Add(mail);
        }

        return mailList;
    }
}

