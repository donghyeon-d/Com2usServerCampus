using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.RequestResponse;
namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StageListController : ControllerBase
{
    readonly ILogger<StageListController> _logger;
    readonly ICompletedDungeonDb _dungeonStageDb;

    public StageListController(ILogger<StageListController> logger,
        ICompletedDungeonDb dungeonStageDb)
    {
        _logger = logger;
        _dungeonStageDb = dungeonStageDb;
    }

    [HttpPost]
    public async Task<StageListRes> ReadCompletedDungeonList()
    {
        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        StageListRes response = new();

        var (readCompleteListErrorCode, currentStage)
                = await _dungeonStageDb.ReadCompleteList(playerId);
        if ( readCompleteListErrorCode != ErrorCode.None)
        {
            response.Result = readCompleteListErrorCode;
            return response;
        }

        response.CompleteList = currentStage;
        
        return response;
    }
}
