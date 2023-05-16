using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.RequestResponse;
using DungeonAPI.ModelDB;
using System.Collections.Generic;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StageListController : ControllerBase
{
    readonly ILogger<StageListController> _logger;
    readonly ICompletedDungeonDb _completedDungeonDb;

    public StageListController(ILogger<StageListController> logger,
        ICompletedDungeonDb completedDungeonDb)
    {
        _logger = logger;
        _completedDungeonDb = completedDungeonDb;
    }

    [HttpPost]
    public async Task<StageListRes> ReadCompletedDungeonList()
    {
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        StageListRes response = new();

        var (readCompleteListErrorCode, completedStageList)
                = await _completedDungeonDb.ReadCompleteList(player.Id);
        if (readCompleteListErrorCode != ErrorCode.None || completedStageList is null)
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
