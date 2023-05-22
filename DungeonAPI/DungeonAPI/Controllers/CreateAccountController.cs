using System.Threading.Tasks;
using DungeonAPI.Services;
using DungeonAPI.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CreateAccountController : ControllerBase
{
    readonly ILogger<CreateAccountController> _logger;
    readonly IAccountDb _accountDb;
    readonly IGameDb _gameDb;

    public CreateAccountController(ILogger<CreateAccountController> logger, 
                IAccountDb accountDb, IGameDb game)
    {
        _logger = logger;
        _accountDb = accountDb;
        _gameDb = game;
    }

    [HttpPost]
    public async Task<CreateAccountRes> CreateAccountThenDefaultData(CreateAccountReq request)
    {
        var response = new CreateAccountRes();

        var (accountErrorCode, accountId) = await _accountDb.CreateAccountAsync(request.Email, request.Password);
        if (accountErrorCode != ErrorCode.None)
        {
            response.Result = accountErrorCode;
            _logger.ZLogInformationWithPayload(new { Email = request.Email }, response.Result.ToString());
            return response;
        }

        var (createPlayerErrorCode, playerId) = await CreatePlayerAsync(request.Email, accountId);
        if (createPlayerErrorCode != ErrorCode.None)
        {
            response.Result = createPlayerErrorCode;
            _logger.ZLogInformationWithPayload(new { Email = request.Email }, response.Result.ToString());
            return response;
        }

        _logger.ZLogInformationWithPayload(new { PlayerId = playerId }, response.Result.ToString());
        return response;
    }

    async Task<Tuple<ErrorCode, Int32>> CreatePlayerAsync(string email, int accountId)
    {

        var (playerErrorCode, playerId) = await _gameDb.CreatePlayerAsync(accountId);
        if (playerErrorCode != ErrorCode.None)
        {
            await Rollback(email, accountId);
            _logger.ZLogInformationWithPayload(new { Email = email }, playerErrorCode.ToString());
            return new (playerErrorCode, 0);
        }

        var itemErrorCode = await _gameDb.CreateDefaltItemsAsync(playerId);
        if (itemErrorCode != ErrorCode.None)
        {
            await Rollback(email, accountId, playerId);
            _logger.ZLogInformationWithPayload(new { Email = email, PlayerID = playerId }, itemErrorCode.ToString());
            return new (itemErrorCode, playerId);
        }

        var createPlayerAttendanceBookErrorCode = await _gameDb.CreatePlayerAttendanceBook(playerId);
        if (createPlayerAttendanceBookErrorCode != ErrorCode.None)
        {
            await Rollback(email, accountId, playerId, playerId);
            _logger.ZLogInformationWithPayload(new { Email = email, PlayerID = playerId }, createPlayerAttendanceBookErrorCode.ToString());
            return new (createPlayerAttendanceBookErrorCode, 0);
        }

        return new(ErrorCode.None, playerId);
    }

    async Task Rollback(String email = "", Int32 accountId = 0, Int32 playerId = 0, Int32 attendancePlayerId = 0)
    {
        if (string.IsNullOrEmpty(email) == false)
        {
            var deleteAccountErrorCode = await _accountDb.DeleteAccountAsync(email);
            if (deleteAccountErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(new { Email = email }, deleteAccountErrorCode.ToString());
            }
        }

        if (accountId != 0) 
        {
            var deletePlayerErrorCode = await _gameDb.DeletePlayerAsync(accountId);
            if (deletePlayerErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(new { Email = email }, deletePlayerErrorCode.ToString());
            }
        }

        if (playerId != 0)
        {
            var deletedPlayerItemErrorCode = await _gameDb.DeletePlayerAllItemsAsync(playerId);
            if (deletedPlayerItemErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, deletedPlayerItemErrorCode.ToString());
            }
        }

        if (attendancePlayerId != 0)
        {
            var deletePlayerAttendanceBookErrorCode = await _gameDb.DeletePlayerAttendanceBook(playerId);
            if (deletePlayerAttendanceBookErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, deletePlayerAttendanceBookErrorCode.ToString());
            }
        }
    }
}

