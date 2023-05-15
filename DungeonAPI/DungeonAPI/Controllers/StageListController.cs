using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.RequestResponse;
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
        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        StageListRes response = new();

        var (readCompleteListErrorCode, currentStage)
                = await _completedDungeonDb.ReadCompleteList(playerId);
        if ( readCompleteListErrorCode != ErrorCode.None)
        {
            response.Result = readCompleteListErrorCode;
            return response;
        }

        response.CompleteList = currentStage;
        
        return response;
    }
}
