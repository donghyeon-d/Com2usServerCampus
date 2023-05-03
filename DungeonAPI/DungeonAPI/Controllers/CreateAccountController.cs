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
    readonly IMasterDataDb _masterData;
    readonly IUser _user;
    readonly IInventory _inventory;

    public CreateAccountController(ILogger<CreateAccountController> logger, IAccountDb accountDb,
        IMasterDataDb masterData, IUser user, IInventory inventory)
    {
        _logger = logger;
        _accountDb = accountDb;
        _masterData = masterData;
        _user = user;
        _inventory = inventory;
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

        // 게임 데이터 User 생성
        var (userErrorCode, userId) = await _user.CreateUserAsync(accountId);
        if (userErrorCode != ErrorCode.None)
        {
            await _accountDb.DeleteAccountAsync(request.Email);
            await _user.DeleteUserAsync(accountId);
            response.Result = userErrorCode;
            return response;
        }

        // 게임 기본 아이템 생성
        var inventoryErrorCode = await _inventory.CreateDefaltItemsAsync(userId);
        if (inventoryErrorCode != ErrorCode.None)
        {
            await _accountDb.DeleteAccountAsync(request.Email);
            await _user.DeleteUserAsync(accountId);
            await _inventory.DeleteUserAllItemsAsync(userId);
            response.Result = inventoryErrorCode;
            return response;
        }

        //_logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], new { Email = request.Email }, $"CreateAccount Success");
        return response;
    }
}

