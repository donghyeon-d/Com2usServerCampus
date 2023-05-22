using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReceiveMailItemController : ControllerBase
{
    readonly ILogger<ReceiveMailItemController> _logger;
    readonly IGameDb _gameDb;

    public ReceiveMailItemController(ILogger<ReceiveMailItemController> logger,
        IGameDb mailDb)
	{
        _logger = logger;
        _gameDb = mailDb;
	}

    [HttpPost]
    public async Task<ReceiveMailItemRes> ProcessRequest(ReceiveMailItemReq request)
    {
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        ReceiveMailItemRes response = await ReceiveMailItem(request, player.Id);

        _logger.ZLogInformationWithPayload(new { Player = player.Id }, response.Result.ToString());

        return response;
    }

    async Task<ReceiveMailItemRes> ReceiveMailItem(ReceiveMailItemReq request, Int32 playerId)
    {

        ReceiveMailItemRes response = new() { Result = ErrorCode.None };

        var (loadMailErrorCode, mail) = await LoadMail(playerId, request.MailId);
        if (loadMailErrorCode != ErrorCode.None || mail is null)
        {
            response.Result = loadMailErrorCode;
            return response;
        }

        var markAsReceivedErrorCode = await _gameDb.MarkAsReceivedItem(request.MailId, playerId);
        if (markAsReceivedErrorCode != ErrorCode.None)
        {
            response.Result = markAsReceivedErrorCode;
            return response;
        }

        var pushItemToListErrorCode = await PushMailItemToList(playerId, mail);
        if (pushItemToListErrorCode != ErrorCode.None)
        {
            var rollbackErrorCode = await _gameDb.MarkAsNotReceivedItem(request.MailId, playerId);
            if (rollbackErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(new { Player = playerId }, "RollBackError " + rollbackErrorCode.ToString());
            }
            response.Result = pushItemToListErrorCode;
            return response;
        }

        return response;
    }

    async Task<Tuple<ErrorCode, Mail?>> LoadMail(Int32 playerId, Int32 mailId)
    {
        var (loadMailErrorCode, mail) = await _gameDb.LoadMail(mailId);
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
        Item? item = Item.InitItem(playerId, mail.ItemCode1, mail.ItemCount1);
        if (item is null)
        {
            return ErrorCode.InvalidItemCode;
        }

        var (pushItemToListErrorCode, itemId) = await _gameDb.AddItemToPlayerItemList(playerId, item);
        if (pushItemToListErrorCode != ErrorCode.None || itemId < 1)
        {
            return pushItemToListErrorCode;
        }

        return ErrorCode.None;
    }
}

