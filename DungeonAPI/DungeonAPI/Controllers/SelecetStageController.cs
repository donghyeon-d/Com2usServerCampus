using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;
using System;
using DungeonAPI.Enum;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SelecetStageController : ControllerBase
{
    readonly ILogger<SelecetStageController> _logger;
    readonly ICompletedDungeonDb _dungeonStageDb;
    readonly IMemoryDb _memoryDb;

    public SelecetStageController(ILogger<SelecetStageController> logger,
        ICompletedDungeonDb dungeonStageDb, IMemoryDb memoryDb)
    {
        _logger = logger;
        _dungeonStageDb = dungeonStageDb;
        _memoryDb = memoryDb;

    }

    [HttpPost]
    public async Task<SelecetStageRes> SelectStage(SelecetStageReq request)
    {
        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());
        string playerStatus = HttpContext.Items["PlayerStatus"].ToString();
        Int32 playerStage = int.Parse(HttpContext.Items["PlayerStage"].ToString());

        SelecetStageRes response = new();

        var checkCanEnterStage = await CheckCanEnterStage(playerId, request.StageCode);
        if (checkCanEnterStage != ErrorCode.None)
        {
            response.Result = checkCanEnterStage;
            return response;
        }

        var changeUserStatusErrorCode
            = await _memoryDb.ChangeUserStatus(request.Email, PlayerStatus.DungeonPlay, request.StageCode);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            response.Result = changeUserStatusErrorCode;
            return response;
        }

        response.ItemList = InitItemList(request.StageCode);
        response.NPCList = InitNPCList(request.StageCode);
        return response;
    }

    async Task<ErrorCode> CheckCanEnterStage(Int32 playerId, Int32 stageCode)
    {
        var requestStageInfo = MasterDataDb.s_stage.Find(stage => stage.StageCode == stageCode);
        if (requestStageInfo is null)
        {
            return ErrorCode.InvalidStageCode;
        }

        var (readCompleteThemaListErrorCode, themaCompeteStageList)
              = await _dungeonStageDb.ReadCompleteThemaList(playerId, requestStageInfo.Thema);
        if (readCompleteThemaListErrorCode != ErrorCode.None || themaCompeteStageList is null)
        {
            return readCompleteThemaListErrorCode;
        }

        if (themaCompeteStageList.Count == 0)
        {
            if (IsFirstStage(requestStageInfo))
            {
                return ErrorCode.None;
            }
            return ErrorCode.NeedToCompleteBeforeStage;
        }

        if (IsCompeteBeforeStage(requestStageInfo, themaCompeteStageList))
        {
            return ErrorCode.None;
        }
        return ErrorCode.NotCompleteBeforeStage;
    }

    bool IsFirstStage(MasterData.DungeonStage stageInfo)
    {
        if (stageInfo.Stage == 1)
        {
            return true;
        }
        return false;
    }

    bool IsCompeteBeforeStage(MasterData.DungeonStage stageInfo, List<CompletedDungeon> themaCompleteStageList)
    {
        if (stageInfo.Stage == 1)
        {
            return true;
        }

        var beforeStage = themaCompleteStageList.Find(Stage => Stage.Stage == stageInfo.Stage - 1);
        if (beforeStage is null)
        {
            return false;
        }

        return true;
    }

    List<FarmingItem> InitItemList(Int32 stageCode)
    {
        List<FarmingItem> list = new();

        var stageItemList = MasterDataDb.s_stageItem.FindAll(item => item.StageCode == stageCode);

        foreach ( var item in stageItemList )
        {
            list.Add( new() { 
                ItemCode = item.ItemCode, 
                Count = item.Count 
            });
        }

        return list;
    }

    List<KillNPC> InitNPCList(Int32 stageCode)
    {
        List<KillNPC> list = new();

        var stageNPCList = MasterDataDb.s_stageAttackNPC.FindAll(item => item.StageCode == stageCode);

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


    async Task RollbackUserStatus(string email)
    {
        var changeUserStatusErrorCode
            = await _memoryDb.ChangeUserStatus(email, PlayerStatus.LogIn);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            // TODO : Log
        }

    }
}
