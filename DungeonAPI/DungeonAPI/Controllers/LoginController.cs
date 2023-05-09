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
    readonly IAuthUserDb _authUser;
    readonly IPlayerDb _player;
    readonly IItemDb _item;

    public LoginController(ILogger<LoginController> logger, IAccountDb accountDb,
        IPlayerDb player, IItemDb item, IAuthUserDb authUser) 
	{
        _logger = logger;
        _accountDb = accountDb;
        _player = player;
        _item = item;
        _authUser = authUser;
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

        var (loadPlayerErrorCode, player) = await _player.LoadPlayerByAccountAsync(accountId);
        if (loadPlayerErrorCode != ErrorCode.None)
        {
            response.ResetThenSetErrorCode(loadPlayerErrorCode);
            return response;
        }
        response.Player = player;

        var (loadItemErrorcode, items) = await _item.LoadAllItemsAsync(player.PlayerId);
        if (loadItemErrorcode != ErrorCode.None)
        {
            response.ResetThenSetErrorCode(loadItemErrorcode);
            return response;
        }
        response.Item = items;

        var authToken = Security.CreateAuthToken();

        var authCheckErrorCode = await _authUser.CreateAuthUserAsync(request.Email, authToken, player.PlayerId);
        if (authCheckErrorCode != ErrorCode.None)
        {
            response.ResetThenSetErrorCode(authCheckErrorCode);
            return response;
        }
        response.AuthToken = authToken;

        return response;
    }
}

