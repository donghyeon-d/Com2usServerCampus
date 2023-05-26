using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.RequestResponse;
using DungeonAPI.ModelDB;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StageListController : ControllerBase
{
    readonly ILogger<StageListController> _logger;
    readonly IGameDb _gameDb;

    public StageListController(ILogger<StageListController> logger,
        IGameDb gameDb)
    {
        _logger = logger;
        _gameDb = gameDb;
    }

    [HttpPost]
    public async Task<StageListRes> ProcessRequest(StageListReq request)
    {
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        StageListRes response = await ReadCompletedDungeonList(player);

        _logger.ZLogInformationWithPayload(new { PlayerId = player.Id }, response.Result.ToString());

        return response;
    }

    async Task<StageListRes> ReadCompletedDungeonList(PlayerInfo player)
    {
        StageListRes response = new();

        var (readCompleteListErrorCode, completedStage)
                = await _gameDb.ReadCompleteList(player.Id);
        if (readCompleteListErrorCode != ErrorCode.None || completedStage is null)
        {
            response.Result = readCompleteListErrorCode;
            return response;
        }

        response.CompleteStage = completedStage;
        return response;
    }
}
