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
                IAccountDb accountDb, IMasterDataDb masterData, IGameDb game)
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

        var (playerErrorCode, playerId) = await _gameDb.CreatePlayerAsync(accountId);
        if (playerErrorCode != ErrorCode.None)
        {
            await Rollback(request.Email, accountId);
            response.Result = playerErrorCode;
            _logger.ZLogInformationWithPayload(new { Email = request.Email }, response.Result.ToString());
            return response;
        }

        var itemErrorCode = await _gameDb.CreateDefaltItemsAsync(playerId);
        if (itemErrorCode != ErrorCode.None)
        {
            await Rollback(request.Email, accountId, playerId);
            response.Result = itemErrorCode;
            _logger.ZLogInformationWithPayload(new { Email = request.Email }, response.Result.ToString());
            return response;
        }

        var createPlayerAttendanceBookErrorCode = await _gameDb.CreatePlayerAttendanceBook(playerId);
        if (createPlayerAttendanceBookErrorCode != ErrorCode.None)
        {
            await Rollback(request.Email, accountId, playerId, playerId);
            response.Result = createPlayerAttendanceBookErrorCode;
            _logger.ZLogInformationWithPayload(new { Email = request.Email }, response.Result.ToString());
            return response;
        }

        _logger.ZLogInformationWithPayload(new { Email = request.Email }, response.Result.ToString());
        return response;
    }

    async Task Rollback(String email = "", Int32 accountId = 0, Int32 playerId = 0, Int32 attendancePlayerId = 0)
    {
        if (string.IsNullOrEmpty(email) == false)
        {
            await _accountDb.DeleteAccountAsync(email);
        }

        if (accountId != 0) 
        { 
            await _gameDb.DeletePlayerAsync(accountId);
        }

        if (playerId != 0)
        {
            await _gameDb.DeletePlayerAllItemsAsync(playerId);
        }

        if (attendancePlayerId != 0)
        {
            await _gameDb.DeletePlayerAttendanceBook(playerId);
        }
    }
}

