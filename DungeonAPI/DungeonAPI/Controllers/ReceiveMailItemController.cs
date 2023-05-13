using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReceiveMailItemController : ControllerBase
{
    readonly ILogger<ReceiveMailItemController> _logger;
    readonly IMailDb _mailDb;
    readonly IItemDb _itemDb;

    public ReceiveMailItemController(ILogger<ReceiveMailItemController> logger,
        IMailDb mailDb, IItemDb itemDb)
	{
        _logger = logger;
        _mailDb = mailDb;
        _itemDb = itemDb;
	}

    [HttpPost]
    public async Task<ReceiveMailItemRes> ReceiveMailItem(ReceiveMailItemReq request)
    {
        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        ReceiveMailItemRes response = new ();

        var (loadMailErrorCode, mail) = await LoadMail(playerId, request.MailId);
        if (loadMailErrorCode != ErrorCode.None || mail is null)
        {
            response.Result = loadMailErrorCode;
            return response;
        }

        var markAsReceivedErrorCode = await _mailDb.MarkAsReceivedItem(request.MailId, playerId);
        if (markAsReceivedErrorCode != ErrorCode.None)
        {
            response.Result = markAsReceivedErrorCode;
            return response;
        }

        var pushItemToListErrorCode = await PushMailItemToList(playerId, mail);
        if (pushItemToListErrorCode != ErrorCode.None)
        {
            response.Result = pushItemToListErrorCode;
            return response;
        }

        return response;
    }

    async Task<Tuple<ErrorCode, Mail?>> LoadMail(Int32 playerId, Int32 mailId)
    {
        var (loadMailErrorCode, mail) = await _mailDb.LoadMail(mailId);
        if (loadMailErrorCode != ErrorCode.None || mail is null)
        {
            return new (loadMailErrorCode, null);
        }

        if (mail.PlayerId != playerId)
        {
            return new (ErrorCode.LoadMailWrongPlayer, null);
        }

        return new(ErrorCode.None, mail);
    }

    async Task<ErrorCode> PushMailItemToList(Int32 playerId, Mail mail)
    {
        Item? item = Item.InitItem(playerId, mail.ItemCode, mail.ItemCount);
        if (item is null)
        {
            return ErrorCode.InvalidItemCode;
        }

        var (pushItemToListErrorCode, itemId) = await _itemDb.AddItemToPlayerItemList(playerId, item);
        if (pushItemToListErrorCode != ErrorCode.None || itemId < 1)
        {
            return pushItemToListErrorCode;
        }

        return ErrorCode.None;
    }
}

