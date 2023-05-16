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
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        SelecetStageRes response = new();

        var checkCanEnterStage = await CheckCanEnterStage(player.Id, request.StageCode);
        if (checkCanEnterStage != ErrorCode.None)
        {
            response.Result = checkCanEnterStage;
            return response;
        }

        var initDungeonInfoErrorCode = await MemorizeDungeonInfo(request.Email, request.StageCode);
        if (initDungeonInfoErrorCode != ErrorCode.None)
        {
            response.Result = initDungeonInfoErrorCode;
            return response;
        }

        var changeUserStatusErrorCode
            = await ChangeUserStatus(request.Email, player, request.StageCode);
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

        var beforeStage = themaCompleteStageList.Find(Stage => Stage.StageCode == stageInfo.Stage - 1);
        if (beforeStage is null)
        {
            return false;
        }

        return true;
    }

    async Task<ErrorCode> MemorizeDungeonInfo(string email, Int32 stageCode)
    {
        InDungeon dungeonInfo = InitDungeonInfo(stageCode);

        var setDungeonInfoErrorCode = await _memoryDb.SetDungeonInfo(email, dungeonInfo);
        if (setDungeonInfoErrorCode != ErrorCode.None)
        {
            return setDungeonInfoErrorCode;
        }

        return ErrorCode.None;
    }

    InDungeon InitDungeonInfo(Int32 stageCode)
    {
        InDungeon dungeonInfo = new()
        {
            KillNPCList = InitNPCList(stageCode),
            ItemList = InitItemList(stageCode)
        };

        return dungeonInfo;
    }

    async Task<ErrorCode> ChangeUserStatus(string email, PlayerInfo player, Int32 stageCode)
    {
        player.Status = PlayerStatus.DungeonPlay.ToString();
        player.currentStage = stageCode;

        var changeUserStatusErrorCode
            = await _memoryDb.UpdateUserStatus(email, player);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            return changeUserStatusErrorCode;
        }

        return ErrorCode.None;
    }

    List<FarmingItem> InitItemList(Int32 stageCode)
    {
        List<FarmingItem> list = new();

        var stageItemList = MasterDataDb.s_stageItem.FindAll(item => item.StageCode == stageCode);

        foreach ( var item in stageItemList )
        {
            list.Add( new() { 
                ItemCode = item.ItemCode, 
                Max = item.Count 
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
                Max = NPC.NPCCount
            });
        }

        return list;
    }
}
