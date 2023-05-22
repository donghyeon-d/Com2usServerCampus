using System;
using DungeonAPI.Services;
using DungeonAPI.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.Util;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    readonly ILogger<LoginController> _logger;
    readonly IAccountDb _accountDb;
    readonly IMemoryDb _memoryDb;
    readonly IGameDb _gameDb;

    public LoginController(ILogger<LoginController> logger, IAccountDb accountDb,
        IGameDb gameDb, IMemoryDb memoryDb) 
	{
        _logger = logger;
        _accountDb = accountDb;
        _gameDb = gameDb;
        _memoryDb = memoryDb;

    }

    [HttpPost]
    public async Task<LoginRes> ProcessRequest(LoginReq request)
    {
        LoginRes response = await TryLogin(request);
        _logger.ZLogInformationWithPayload(new { Email = request.Email }, response.Result.ToString());

        return response;
    }

    async Task<LoginRes> TryLogin(LoginReq request)
    {
        LoginRes response = new LoginRes();

        var (accountErrorCode, accountId) = await _accountDb.VerifyAccountAsync(request.Email, request.Password);
        if (accountErrorCode != ErrorCode.None)
        {
            response.Result = accountErrorCode;
            return response;
        }

        var (loadPlayerErrorCode, player) = await _gameDb.LoadPlayerByAccountAsync(accountId);
        if (loadPlayerErrorCode != ErrorCode.None || player is null)
        {
            response.Result = loadPlayerErrorCode;
            return response;
        }

        var (loadItemErrorcode, items) = await _gameDb.LoadPlayerItemListAsync(player.PlayerId);
        if (loadItemErrorcode != ErrorCode.None)
        {
            response.Result = loadItemErrorcode;
            return response;
        }

        var authToken = Security.CreateAuthToken();

        var authCheckErrorCode = await _memoryDb.CreatePlayerInfo(player.PlayerId, authToken);
        if (authCheckErrorCode != ErrorCode.None)
        {
            response.Result = authCheckErrorCode;
            return response;
        }

        response.Player = player;
        response.Item = items;
        response.AuthToken = authToken;

        return response;
    }
}

