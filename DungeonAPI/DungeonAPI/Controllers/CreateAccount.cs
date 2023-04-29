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

    public CreateAccountController(ILogger<CreateAccountController> logger, IAccountDb accountDb, IMasterDataDb masterData)
    {
        _logger = logger;
        _accountDb = accountDb;
        _masterData = masterData;
    }

    [HttpPost]
    public async Task<CreateAccountRes> Post(CreateAccountReq request)
    {
        var response = new CreateAccountRes();

        var errorCode = await _accountDb.CreateAccountAsync(request.Email, request.Password);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        //_logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], new { Email = request.Email }, $"CreateAccount Success");
        return response;
    }
}

