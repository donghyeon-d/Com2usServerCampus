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

        StageListRes response = await ReadCompletedDungeonList(request, player);

        _logger.ZLogInformationWithPayload(new { Email = request.Email }, response.Result.ToString());

        return response;
    }

    async Task<StageListRes> ReadCompletedDungeonList(StageListReq request, PlayerInfo player)
    {
        StageListRes response = new();

        var (readCompleteListErrorCode, completedStageList)
                = await _gameDb.ReadCompleteList(player.Id);
        if (readCompleteListErrorCode != ErrorCode.None ||
            completedStageList is null || completedStageList.Count() == 0)
        {
            response.Result = readCompleteListErrorCode;
            return response;
        }

        response.StageCodeList = InitStageList(completedStageList);
        return response;
    }

    List<Int32> InitStageList(List<CompletedDungeon> completedStageList)
    {
        List<Int32> stageCodeList = new();
        foreach (var completedDungeon in completedStageList)
        {
            stageCodeList.Add(completedDungeon.StageCode);
        }

        return stageCodeList;
    }
}
