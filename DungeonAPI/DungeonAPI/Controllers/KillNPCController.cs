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
        KillNPCRes response = new();

        string playerStatus = HttpContext.Items["PlayerStatus"].ToString();
        Int32 playerStage = int.Parse(HttpContext.Items["PlayerStage"].ToString());

        var checkValidRequestErrorCode = CheckValidRequest(request, playerStatus, playerStage);
        if (checkValidRequestErrorCode != ErrorCode.None)
        {
            response.Result = checkValidRequestErrorCode;
            await SetExitDungeon(request.Email);
            return response;
        }

        var (getKillNPCErrorCode, NPCList)
            = await GetKillNPCList(request.Email);
        if (getKillNPCErrorCode != ErrorCode.None || NPCList is null)
        {
            response.Result = getKillNPCErrorCode;
            return response;
        }

        var addKillNPCToListErrorCode = await AddKilledNPCToList(request.Email, NPCList, request.KilledNPCCode);
        if (addKillNPCToListErrorCode != ErrorCode.None)
        {
            response.Result = addKillNPCToListErrorCode;
            await SetExitDungeon(request.Email);
            return response;
        }

        return response;
    }

    async Task<Tuple<ErrorCode, List<KillNPC>?>>GetKillNPCList(string email)
    {
        var (getKillNPCErrorCode, NPCList) = await _memoryDb.GetKillNPCList(email);
        if (getKillNPCErrorCode == ErrorCode.GetKillNPCNotExist)
        {
            return new(ErrorCode.None, new List<KillNPC>());
        }

        if (getKillNPCErrorCode != ErrorCode.None || NPCList is null)
        {
            return new(getKillNPCErrorCode, null);
        }


        return new(ErrorCode.None, NPCList);
    }

    ErrorCode CheckValidRequest(KillNPCReq request, string playerStatus, Int32 playerStage)
    {
        if (IsPlayerInDungeon(playerStatus) == false)
        {
            return ErrorCode.InvalidPlayerStatusNotPlayDungeon;
            
        }

        if (IsVaildStageNPC(playerStage, request.KilledNPCCode) == false)
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

    async Task<ErrorCode> AddKilledNPCToList(string email, List<KillNPC> NPCList, Int32 NPCCode)
    {
        var NPCToListErrorCode = NPCToList(NPCList, NPCCode);
        if (NPCToListErrorCode != ErrorCode.None)
        {
            return NPCToListErrorCode;
        }

        var setKillNPCErrorCode = await _memoryDb.SetKillNPCList(email, NPCList);
        if (setKillNPCErrorCode != ErrorCode.None)
        {
            return setKillNPCErrorCode;
        }

        return ErrorCode.None;
    }

    ErrorCode NPCToList(List<KillNPC> NPCList, Int32 NPCCode)
    {
        int index = NPCList.FindIndex(NPC => NPC.NPCCode == NPCCode);
        if (index == -1)
        {
            NPCList.Add(new() { NPCCode = NPCCode, Count = 1 });
        }
        else
        {
            if (CanAddNPCToKillList(NPCList[index], NPCCode) == false)
            {
                return ErrorCode.TooMuchKillNPC;
            }
            else
            {
                NPCList[index].Count++;
            }
        }
        return ErrorCode.None;
    }

    bool CanAddNPCToKillList(KillNPC NPC, Int32 NPCCode)
    {
        if (NPC.Count == MaxNPCKillCount(NPCCode))
        {
            return false;
        }
        return true;
    }

    int MaxNPCKillCount(Int32 NPCCode)
    {
        var baseNPC = MasterDataDb.s_stageAttackNPC.Find(npc => npc.NPCCode == NPCCode);
        return baseNPC.NPCCount;
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
