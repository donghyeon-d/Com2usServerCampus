using DungeonAPI.ModelDB;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class KillNPCController : ControllerBase
{
    readonly ILogger<KillNPCController> _logger;
    readonly IMemoryDb _memoryDb;

    public KillNPCController(ILogger<KillNPCController> logger,
        IMemoryDb memoryDb)
    {
        _logger = logger;
        _memoryDb = memoryDb;
    }

    [HttpPost]
    public async Task<KillNPCRes> AddKillNPCToList(KillNPCReq request)
    {
        KillNPCRes response = new();

        string playerStatus = HttpContext.Items["PlayerStatus"].ToString();
        Int32 playerStage = int.Parse(HttpContext.Items["PlayerStage"].ToString());

        if (playerStatus != PlayerStatus.DungeonPlay.ToString())
        {
            response.Result = ErrorCode.InvalidPlayerStatusNotPlayStage;
            return response;
        }

        if (IsVaildStageNPC(playerStage, request.KilledNPCCode) == false)
        {
            response.Result = ErrorCode.InvalidStageNPC;
            return response;
        }

        var (getKillNPCErrorCode, NPCList)
            = await _memoryDb.GetKillNPCList(request.Email);
        if (getKillNPCErrorCode != ErrorCode.None || NPCList is null)
        {
            response.Result = getKillNPCErrorCode;
            return response;
        }

        NPCList.Add(request.KilledNPCCode); ///
        var setKillNPCErrorCode = await _memoryDb.SetKillNPCList(request.Email, NPCList);
        if (setKillNPCErrorCode == ErrorCode.None)
        {
            response.Result = setKillNPCErrorCode;
            return response;
        }
    }

    bool IsVaildStageNPC(Int32 playerStage, Int32 killedNPCCode)
    {
        var stageNPCs = MasterDataDb.s_stageAttackNPC.FindAll(item => item.StageCode == playerStage);
        if (stageNPCs is null || stageNPCs.Count == 0)
        {
            return false;
        }

        foreach ( var stageNPC in stageNPCs)
        {
            if (stageNPC.NPCCode == killedNPCCode)
            { 
                return true; 
            }
        }
        return false;
    }


}
