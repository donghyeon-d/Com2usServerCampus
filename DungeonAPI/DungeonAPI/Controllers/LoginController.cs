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
    readonly ImemoryDb _memoryDb;
    readonly IUser _user;
    readonly IInventory _inventory;

    public LoginController(ILogger<LoginController> logger, IAccountDb accountDb,
        IUser user, IInventory inventory, ImemoryDb memoryDb) 
	{
        _logger = logger;
        _accountDb = accountDb;
        _user = user;
        _inventory = inventory;
        _memoryDb = memoryDb;
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

        // token 만들기
        var authToken = Security.CreateAuthToken();

        // token redis에 저장하기
        var authCheckErrorCode = await _memoryDb.CreateAuthUserAsync(request.Email, authToken);
        if (authCheckErrorCode != ErrorCode.None)
        {
            response.ResetThenSetErrorCode(authCheckErrorCode);
            return response;
            
        }
        response.AuthToken = authToken;

        // Load user data
        var (loadUserErrorCode, user) = await _user.LoadUserByAccountAsync(accountId);
        if (loadUserErrorCode != ErrorCode.None)
        {
            // TODO : 롤백
            response.ResetThenSetErrorCode(loadUserErrorCode);
            return response;
        }
        response.User = user;

        // Load item data
        var (loadItemErrorcode, items) = await _inventory.LoadAllItemsAsync(user.UserId);
        if (loadItemErrorcode != ErrorCode.None)
        {
            // TODO : 롤백
            response.ResetThenSetErrorCode(loadItemErrorcode);
            return response;
        }
        response.Item = items;

        return response;
    }
}

