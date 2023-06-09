﻿using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;
using DungeonAPI.Enum;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SelectStageController : ControllerBase
{
    readonly ILogger<SelectStageController> _logger;
    readonly IGameDb _gameDb;
    readonly IMemoryDb _memoryDb;

    public SelectStageController(ILogger<SelectStageController> logger,
        IGameDb gameDb, IMemoryDb memoryDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _memoryDb = memoryDb;
    }

    [HttpPost]
    public async Task<SelectStageRes> SelectStage(SelectStageReq request)
    {
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        SelectStageRes response = await SelectStage(request, player);

        _logger.ZLogInformationWithPayload(new { PlayerId = player.Id, StageCode = request.StageCode }, response.Result.ToString());

        return response;
    }

    async Task<SelectStageRes> SelectStage(SelectStageReq request, PlayerInfo player)
    {
        SelectStageRes response = new();

        var checkCanEnterStage = await _gameDb.CheckCanEnterStage(player.Id, request.StageCode);
        if (checkCanEnterStage != ErrorCode.None)
        {
            response.Result = checkCanEnterStage;
            return response;
        }

        var initDungeonInfoErrorCode = await MemorizeDungeonInfo(player.Id, request.StageCode);
        if (initDungeonInfoErrorCode != ErrorCode.None)
        {
            response.Result = initDungeonInfoErrorCode;
            return response;
        }

        var changeUserStatusErrorCode = await ChangeUserStatus(player, request.StageCode);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            await RollbackDungeonInfo(player.Id);
            response.Result = changeUserStatusErrorCode;
            return response;
        }

        InitResponse(response, request.StageCode);

        return response;
    }


    async Task RollbackDungeonInfo(Int32 playerId)
    {
        var deleteDungeonInfo = await _memoryDb.DeleteDungeonInfo(playerId);
        if (deleteDungeonInfo != ErrorCode.None)
        {
            _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, "RollBackError " + deleteDungeonInfo.ToString());
        }
    }

    void InitResponse(SelectStageRes response, Int32 stageCode)
    {
        response.ItemList = InitItemList(stageCode);
        response.NPCList = InitNPCList(stageCode);
    }

    //async Task<ErrorCode> CheckCanEnterStage(Int32 playerId, Int32 stageCode)
    //{
    //    var requestStageInfo = MasterDataDb.s_stage.Find(stage => stage.StageCode == stageCode);
    //    if (requestStageInfo is null)
    //    {
    //        return ErrorCode.InvalidStageCode;
    //    }

    //    var (readCompleteThemaListErrorCode, competeStageList)
    //          = await _gameDb.ReadCompleteList(playerId);
    //    if (readCompleteThemaListErrorCode != ErrorCode.None || competeStageList is null)
    //    {
    //        return readCompleteThemaListErrorCode;
    //    }

    //    if (competeStageList.Count == 0)
    //    {
    //        if (IsFirstStage(requestStageInfo))
    //        {
    //            return ErrorCode.None;
    //        }
    //        return ErrorCode.NeedToCompleteBeforeStage;
    //    }

    //    if (IsCompeteBeforeStage(requestStageInfo, competeStageList))
    //    {
    //        return ErrorCode.None;
    //    }
    //    return ErrorCode.NotCompleteBeforeStage;
    //}

    //bool IsFirstStage(MasterData.DungeonStage stageInfo)
    //{
    //    if (stageInfo.Stage == 1)
    //    {
    //        return true;
    //    }
    //    return false;
    //}

    //bool IsCompeteBeforeStage(MasterData.DungeonStage stageInfo, List<CompletedDungeon> themaCompleteStageList)
    //{
    //    if (stageInfo.Stage == 1)
    //    {
    //        return true;
    //    }

    //    var beforeStage = themaCompleteStageList.Find(Stage => Stage.StageCode == stageInfo.StageCode - 1);
    //    if (beforeStage is null)
    //    {
    //        return false;
    //    }

    //    return true;
    //}`

    async Task<ErrorCode> MemorizeDungeonInfo(Int32 playerId, Int32 stageCode)
    {
        InDungeon dungeonInfo = InitDungeonInfo(stageCode);

        var setDungeonInfoErrorCode = await _memoryDb.SetDungeonInfo(playerId, dungeonInfo);
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

    async Task<ErrorCode> ChangeUserStatus(PlayerInfo player, Int32 stageCode)
    {
        player.Status = PlayerStatus.DungeonPlay.ToString();
        player.CurrentStage = stageCode;

        var changeUserStatusErrorCode = await _memoryDb.UpdateUserStatus(player.Id, player);
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
                Max = NPC.Count
            });
        }

        return list;
    }
}
