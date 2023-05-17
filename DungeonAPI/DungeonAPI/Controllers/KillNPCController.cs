using DungeonAPI.Enum;
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
        PlayerInfo player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        KillNPCRes response = new();

        var checkValidRequestErrorCode = CheckValidRequest(player, request);
        if (checkValidRequestErrorCode != ErrorCode.None)
        {
            response.Result = checkValidRequestErrorCode;
            await SetExitDungeon(request.Email);
            return response;
        }

        var (getDungeonInfoErrorCode, dungeonInfo) = await GetDungeonInfo(request.Email);
        if (getDungeonInfoErrorCode != ErrorCode.None || dungeonInfo is null)
        {
            response.Result = getDungeonInfoErrorCode;
            return response;
        }

        var addKillNPCToListErrorCode = await AddKilledNPC(request.Email, dungeonInfo, request.KilledNPCCode);
        if (addKillNPCToListErrorCode != ErrorCode.None)
        {
            response.Result = addKillNPCToListErrorCode;
            await SetExitDungeon(request.Email);
            return response;
        }

        return response;
    }

    async Task<Tuple<ErrorCode, InDungeon?>>GetDungeonInfo(string email)
    {
        var (getDungeonInfoErrorCode, dungeonInfo) = await _memoryDb.GetDungeonInfo(email);
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

        if (IsVaildStageNPC(player.currentStage, request.KilledNPCCode) == false)
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

    async Task<ErrorCode> AddKilledNPC(string email, InDungeon dungeonInfo, Int32 NPCCode)
    {
        var AddNPCCountErrorCode = AddNPCCount(dungeonInfo, NPCCode);
        if (AddNPCCountErrorCode != ErrorCode.None)
        {
            return AddNPCCountErrorCode;
        }

        var setKillNPCErrorCode = await _memoryDb.SetDungeonInfo(email, dungeonInfo);
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

    async Task SetExitDungeon(string email)
    {
        var changeUserStatusErrorCode
            = await _memoryDb.ChangeUserStatus(email, PlayerStatus.LogIn);
        if (changeUserStatusErrorCode != ErrorCode.None)
        {
            // TODO: Rollback Error
        }

        var deleteDungeonInfoErrorCode = await _memoryDb.DeleteDungeonInfo(email);
        if (deleteDungeonInfoErrorCode != ErrorCode.None)
        {
            // TODO : Rollback Error
        }
    }
}
