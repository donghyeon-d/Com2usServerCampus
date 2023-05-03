using System;
using DungeonAPI.Services;
using DungeonAPI.MessageBody;
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

        // Load player data
        var (loadPlayerErrorCode, player) = await _player.LoadPlayerByAccountAsync(accountId);
        if (loadPlayerErrorCode != ErrorCode.None)
        {
            response.ResetThenSetErrorCode(loadPlayerErrorCode);
            return response;
        }
        response.Player = player;

        // Load item data
        var (loadItemErrorcode, items) = await _item.LoadAllItemsAsync(player.PlayerId);
        if (loadItemErrorcode != ErrorCode.None)
        {
            response.ResetThenSetErrorCode(loadItemErrorcode);
            return response;
        }
        response.Item = items;

        // token 만들기
        var authToken = Security.CreateAuthToken();

        // token redis에 저장하기
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

