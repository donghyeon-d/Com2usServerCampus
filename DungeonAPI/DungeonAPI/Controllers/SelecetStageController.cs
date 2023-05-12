using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SelecetStageController : ControllerBase
{
    readonly ILogger<SelecetStageController> _logger;
    readonly ICompletedDungeonDb _dungeonStageDb;

    public SelecetStageController(ILogger<SelecetStageController> logger,
        ICompletedDungeonDb dungeonStageDb)
    {
        _logger = logger;
        _dungeonStageDb = dungeonStageDb;
    }


    [HttpPost]
    public async Task<SelecetStageRes> SelectStage(SelecetStageReq request)
    {
        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        SelecetStageRes response = new();

        var checkCanEnterStage = await CheckCanEnterStage(playerId, request.StageCode);
        if (checkCanEnterStage != ErrorCode.None)
        {
            response.Result = checkCanEnterStage;
            return response;
        }

        response.ItemList = InitItemList(request.StageCode);
        response.NPCList = InitNPCList(request.StageCode);
        if (response.ItemList is null || response.NPCList is null)
        {
            response.ItemList = null;
            response.NPCList = null;
            response.Result = ErrorCode.InvalidStageCode;
        }

        return response;
    }

    async Task<ErrorCode> CheckCanEnterStage(Int32 playerId, Int32 stageCode)
    {
        var requestStageInfo = MasterDataDb.s_stage.Find(stage => stage.StageCode == stageCode);
        if (requestStageInfo is null)
        {
            return ErrorCode.InvalidStageCode;
        }

        var (readCompleteThemaListErrorCode, themaStepList)
              = await _dungeonStageDb.ReadCompleteThemaList(playerId, requestStageInfo.Thema);
        if (readCompleteThemaListErrorCode != ErrorCode.None)
        {
            return readCompleteThemaListErrorCode;
        }

        if (themaStepList is null)
        {
            if (IsFirstStep(requestStageInfo))
            {
                return ErrorCode.None;
            }
            return ErrorCode.InvalidStageCode;
        }

        if (IsClearBeforeStage(requestStageInfo, themaStepList))
        {
            return ErrorCode.None;
        }
        return ErrorCode.NotCompleteBeforeStage;
    }

    bool IsFirstStep(MasterData.Stage stageInfo)
    {
        if (stageInfo.Step == 1)
        {
            return true;
        }
        return false;
    }

    bool IsClearBeforeStage(MasterData.Stage stageInfo, List<CompletedDungeon> themaStepList)
    {
        if (stageInfo.Step == 1)
        {
            return true;
        }

        var beforeStep = themaStepList.Find(step => step.Stage == stageInfo.Step - 1);
        if (beforeStep is null)
        {
            return false;
        }

        return true;
    }

    List<StageItem>? InitItemList(Int32 stageCode)
    {
        List<StageItem>? list = new();

        var stageItemList = MasterDataDb.s_stageItem.FindAll(item => item.StageCode == stageCode);
        if (stageItemList is null)
        {
            return null;
        }

        foreach ( var item in stageItemList )
        {
            list.Add( new() { 
                ItemCode = item.ItemCode, 
                Count = item.Count 
            });
        }

        return list;
    }

    List<StageNPC>? InitNPCList(Int32 stageCode)
    {
        List<StageNPC>? list = new();

        var stageNPCList = MasterDataDb.s_stageAttackNPC.FindAll(item => item.StageCode == stageCode);
        if (stageNPCList is null)
        {
            return null;
        }

        foreach (var NPC in stageNPCList)
        {
            list.Add(new()
            {
                NPCCode = NPC.NPCCode,
                Count = NPC.NPCCount
            });
        }

        return list;
    }
}
