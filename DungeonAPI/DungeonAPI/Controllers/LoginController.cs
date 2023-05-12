using System;
using DungeonAPI.Services;
using DungeonAPI.RequestResponse;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    readonly ILogger<LoginController> _logger;
    readonly IAccountDb _accountDb;
    readonly IMemoryDb _memoryDb;
    readonly IPlayerDb _playerDb;
    readonly IItemDb _itemDb;

    public LoginController(ILogger<LoginController> logger, IAccountDb accountDb,
        IPlayerDb player, IItemDb item, IMemoryDb authUser) 
	{
        _logger = logger;
        _accountDb = accountDb;
        _playerDb = player;
        _itemDb = item;
        _memoryDb = authUser;
	}

    [HttpPost]
    public async Task<LoginRes> TryLogin(LoginReq request)
    {
        LoginRes response = new LoginRes();

        var (accountErrorCode, accountId) = await _accountDb.VerifyAccountAsync(request.Email, request.Password);
        if (accountErrorCode != ErrorCode.None)
        {
            response.Result = accountErrorCode;
            return response;
        }

        var (loadPlayerErrorCode, player) = await _playerDb.LoadPlayerByAccountAsync(accountId);
        if (loadPlayerErrorCode != ErrorCode.None || player is null)
        {
            response.Result = loadPlayerErrorCode;
            return response;
        }

        var (loadItemErrorcode, items) = await _itemDb.LoadPlayerItemListAsync(player.PlayerId);
        if (loadItemErrorcode != ErrorCode.None)
        {
            response.Result = loadItemErrorcode;
            return response;
        }

        var authToken = Security.CreateAuthToken();

        var authCheckErrorCode = await _memoryDb.CreateAuthUserAsync(request.Email, authToken, player.PlayerId);
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

