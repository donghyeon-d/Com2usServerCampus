using DungeonAPI.Enum;
using DungeonAPI.ModelDB;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

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
    public async Task<KillNPCRes> ProcessRequest(KillNPCReq request)
    {
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        KillNPCRes response = new();

        response.Result = await AddKillNPCToList(player, request);
        _logger.ZLogInformationWithPayload(new { Player = player.Id, KilledNPCCode = request.KilledNPCCode }, response.Result.ToString());

        return response;
    }

    async Task<ErrorCode> AddKillNPCToList(PlayerInfo player, KillNPCReq request)
    {
        var checkValidRequestErrorCode = CheckValidRequest(player, request);
        if (checkValidRequestErrorCode != ErrorCode.None)
        {
            await SetExitDungeon(player.Id);
            return checkValidRequestErrorCode;
        }

        var (getDungeonInfoErrorCode, dungeonInfo) = await GetDungeonInfo(player.Id);
        if (getDungeonInfoErrorCode != ErrorCode.None || dungeonInfo is null)
        {
            return getDungeonInfoErrorCode;
        }

        var addKillNPCToListErrorCode = await AddKilledNPC(player.Id, dungeonInfo, request.KilledNPCCode);
        if (addKillNPCToListErrorCode != ErrorCode.None)
        {
            await SetExitDungeon(player.Id);
            return addKillNPCToListErrorCode;
        }

        return ErrorCode.None;
    }

    async Task<Tuple<ErrorCode, InDungeon?>>GetDungeonInfo(Int32 playerId)
    {
        var (getDungeonInfoErrorCode, dungeonInfo) = await _memoryDb.GetDungeonInfo(playerId);
        if (getDungeonInfoErrorCode != ErrorCode.None || dungeonInfo is null)
        {
            return new(getDungeonInfoErrorCode, null);
        }

        return new(ErrorCode.None, dungeonInfo);
    }

    ErrorCode CheckValidRequest(PlayerInfo player, KillNPCReq request)
    {
        if (IsPlayerInDungeon(player.Status) == false)
        {
            return ErrorCode.InvalidPlayerStatusNotPlayDungeon;
            
        }

        if (IsVaildStageNPC(player.CurrentStage, request.KilledNPCCode) == false)
        {
            return ErrorCode.InvalidStageNPC;
        }

        return ErrorCode.None;
    }

    bool IsPlayerInDungeon(string playerStatus)
    {
        if (playerStatus != PlayerStatus.DungeonPlay.ToString())
        {
            return false;
        }
        return true;
    }

    bool IsVaildStageNPC(Int32 playerStage, Int32 killedNPCCode)
    {
        var stageNPCs = MasterDataDb.s_stageAttackNPC.FindAll(item => item.StageCode == playerStage);
        if (stageNPCs is null || stageNPCs.Count == 0)
        {
            return false;
        }

        foreach (var stageNPC in stageNPCs)
        {
            if (stageNPC.NPCCode == killedNPCCode)
            {
                return true;
            }
        }
        return false;
    }

    async Task<ErrorCode> AddKilledNPC(Int32 playerId, InDungeon dungeonInfo, Int32 NPCCode)
    {
        var AddNPCCountErrorCode = AddNPCCount(dungeonInfo, NPCCode);

        if (AddNPCCountErrorCode != ErrorCode.None)
        {
            return AddNPCCountErrorCode;
        }

        var setKillNPCErrorCode = await _memoryDb.SetDungeonInfo(playerId, dungeonInfo);
        if (setKillNPCErrorCode != ErrorCode.None)
        {
            return setKillNPCErrorCode;
        }

        return ErrorCode.None;
    }

    ErrorCode AddNPCCount(InDungeon dungeonInfo, Int32 NPCCode)
    {
        var index = dungeonInfo.KillNPCList.FindIndex(NPC => NPC.NPCCode == NPCCode);

        if (IsMaxKillCount(dungeonInfo.KillNPCList[index]))
        {
            return ErrorCode.TooMuchKillNPC;
        }
        else
        {
            AddNPC(dungeonInfo.KillNPCList[index]);
            return ErrorCode.None;
        }
    }

    bool IsMaxKillCount(KillNPC NPC)
    {
        if (NPC.Count == NPC.Max)
        {
            return true;
        }
        return false;
    }

    void AddNPC(KillNPC killNPC)
    {
        killNPC.Count++;
    }

    async Task SetExitDungeon(Int32 playterId)
    {
        var changeUserStatusErrorCode
            = await _memoryDb.ChangeUserStatus(playterId, PlayerStatus.LogIn);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            _logger.ZLogErrorWithPayload(new { PlayerId = playterId }, "RollBackError " + changeUserStatusErrorCode.ToString());
        }

        var deleteDungeonInfoErrorCode = await _memoryDb.DeleteDungeonInfo(playterId);
        if (deleteDungeonInfoErrorCode != ErrorCode.None)
        {
            _logger.ZLogErrorWithPayload(new { PlayerId = playterId }, "RollBackError " + deleteDungeonInfoErrorCode.ToString());

        }
    }
}
