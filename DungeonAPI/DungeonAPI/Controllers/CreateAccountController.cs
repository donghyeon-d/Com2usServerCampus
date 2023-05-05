using System.Threading.Tasks;
using DungeonAPI.Services;
using DungeonAPI.MessageBody;
using Microsoft.AspNetCore.Mvc;


namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CreateAccountController : ControllerBase
{
    readonly ILogger<CreateAccountController> _logger;
    readonly IAccountDb _accountDb;
    readonly IPlayerDb _player;
    readonly IItemDb _item;
    readonly IAttendanceBookDb _attendanceBook;

    public CreateAccountController(ILogger<CreateAccountController> logger, IAccountDb accountDb,
        IMasterDataDb masterData, IPlayerDb player, IItemDb item, IAttendanceBookDb attendanceBook)
    {
        _logger = logger;
        _accountDb = accountDb;
        _player = player;
        _item = item;
        _attendanceBook = attendanceBook;
    }

    [HttpPost]
    public async Task<CreateAccountRes> CreateAccountThenDefaultData(CreateAccountReq request)
    {
        var response = new CreateAccountRes();

        // 계정 생성
        var (accountErrorCode, accountId) = await _accountDb.CreateAccountAsync(request.Email, request.Password);
        if (accountErrorCode != ErrorCode.None)
        {
            response.Result = accountErrorCode;
            await _accountDb.DeleteAccountAsync(request.Email);
            return response;
        }

        // 게임 데이터 Player 생성
        var (playerErrorCode, playerId) = await _player.CreatePlayerAsync(accountId);
        if (playerErrorCode != ErrorCode.None)
        {
            await _accountDb.DeleteAccountAsync(request.Email);
            await _player.DeletePlayerAsync(accountId);
            response.Result = playerErrorCode;
            return response;
        }

        // 게임 기본 아이템 생성
        var itemErrorCode = await _item.CreateDefaltItemsAsync(playerId);
        if (itemErrorCode != ErrorCode.None)
        {
            await _accountDb.DeleteAccountAsync(request.Email);
            await _player.DeletePlayerAsync(accountId);
            await _item.DeletePlayerAllItemsAsync(playerId);
            response.Result = itemErrorCode;
            return response;
        }

        // player의 출석부 생성
        var createPlayerAttendanceBookErrorCode = await _attendanceBook.CreatePlayerAttendanceBook(playerId);
        if (createPlayerAttendanceBookErrorCode != ErrorCode.None)
        {
            await _accountDb.DeleteAccountAsync(request.Email);
            await _player.DeletePlayerAsync(accountId);
            await _item.DeletePlayerAllItemsAsync(playerId);
            await _attendanceBook.DeletePlayerAttendanceBook(playerId);
            response.Result = createPlayerAttendanceBookErrorCode;
            return response;
        }

         //_logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], new { Email = request.Email }, $"CreateAccount Success");
         return response;
    }
}

