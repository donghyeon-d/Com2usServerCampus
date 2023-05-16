using CloudStructures;
using CloudStructures.Structures;
using DungeonAPI.Configs;
using DungeonAPI.Enum;
using DungeonAPI.ModelDB;
using DungeonAPI.Util;
using Microsoft.Extensions.Options;

namespace DungeonAPI.Services;

public class MemoryDb : IMemoryDb
{
    readonly RedisConnection _redisConn;
    readonly ILogger<MemoryDb> _logger;
    readonly IOptions<DbConfig> _dbConfig;
    TimeSpan _defaultExpiry = TimeSpan.FromDays(1);
    TimeSpan _dungeonExpiry = TimeSpan.FromMinutes(5);

    public MemoryDb(ILogger<MemoryDb> logger, IOptions<DbConfig> dbConfig)
	{
        _logger = logger;
        _dbConfig = dbConfig;
        var redisAddress = _dbConfig.Value.Redis;
        var redisConfig = new RedisConfig("default", redisAddress);
        _redisConn = new RedisConnection(redisConfig);

        // TODO: log
    }

    public async Task<ErrorCode> CreatePlayerInfo(string email, string authToken, Int32 playerId)
    {

        var key = KeyMaker.MakePlayerInfo(email);
        // TODO: Expiry setting
        try
        {
            var redis = new RedisString<PlayerInfo>(_redisConn, key, _defaultExpiry);
            PlayerInfo value = new() {
                AuthToken = authToken,
                Id = playerId,
                Status = PlayerStatus.LogIn.ToString() };

            if (await redis.SetAsync(value) == false)
            {
                return ErrorCode.AuthTockenCreateFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.AuthTockenCreateFailException;
        }
    }

    public async Task<ErrorCode> ChangeUserStatus(string email, PlayerStatus status, Int32 stageCode = 0)
    {
        var key = KeyMaker.MakePlayerInfo(email);

        try
        {
            var redis = new RedisString<PlayerInfo>(_redisConn, key, _defaultExpiry);
            var redisValue = await redis.GetAsync();
            if (redisValue.HasValue == false)
            {
                return ErrorCode.ChangeUserNotFound;
            }

            PlayerInfo value = redisValue.Value;
            value.Status = status.ToString();
            value.currentStage = stageCode;

            if (await redis.SetAsync(value) == false)
            {
                return ErrorCode.ChangUserSetStautsFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.ChangUserStatusFailException;
        }
    }

    public async Task<ErrorCode> UpdateUserStatus(string email, PlayerInfo value)
    {
        var key = KeyMaker.MakePlayerInfo(email);

        try
        {
            var redis = new RedisString<PlayerInfo>(_redisConn, key, _defaultExpiry);
            if (await redis.SetAsync(value) == false)
            {
                return ErrorCode.ChangUserSetStautsFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.ChangUserStatusFailException;
        }
    }


    public async Task<ErrorCode> DeleteAuthUserAsync(string email)
    {
        var key = KeyMaker.MakePlayerInfo(email);

        try
        {
            var redis = new RedisString<PlayerInfo>(_redisConn, key, _defaultExpiry);

            if (await redis.DeleteAsync() == false)
            {
                return ErrorCode.DeleteAccountFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.AuthTokenFailException;
        }
    }

    public async Task<Tuple<ErrorCode, PlayerInfo?>> LoadAuthUser(string email)
    {
        var key = KeyMaker.MakePlayerInfo(email);

        try
        {
            var redis = new RedisString<PlayerInfo>(_redisConn, key, _defaultExpiry);
            var result = await redis.GetAsync();
            if (result.HasValue == false)
            {
                return new (ErrorCode.AuthTokenNotFound, null);
            }
            return new (ErrorCode.None, result.Value);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message); //TODO:
            return new (ErrorCode.AuthTokenFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, List<FarmingItem>?>> GetFarmingItemList(string email)
    {
        var key = KeyMaker.MakeFarmingItemKey(email);

        try
        {
            var redis = new RedisString<List<FarmingItem>>(_redisConn, key, _dungeonExpiry);
            var result = await redis.GetAsync();
            if (result.HasValue == false)
            {
                return new(ErrorCode.GetFarmingItemListNotExist, null);
            }

            return new(ErrorCode.None, result.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message); //TODO:
            return new(ErrorCode.GetFarmingItemListFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, List<KillNPC>?>> GetKillNPCList(string email)
    {
        var key = KeyMaker.MakeKillNPCKey(email);

        try
        {
            var redis = new RedisString<List<KillNPC>>(_redisConn, key, _dungeonExpiry);
            var result = await redis.GetAsync();
            if (result.HasValue == false)
            {
                return new(ErrorCode.GetKillNPCNotExist, null);
            }

            return new(ErrorCode.None, result.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message); //TODO:
            return new(ErrorCode.GetKillNPCFailException, null);
        }
    }

    public async Task<ErrorCode> SetFarmingItemList(string email, List<FarmingItem> itemList)
    {
        var key = KeyMaker.MakeFarmingItemKey(email);

        try
        {
            var redis = new RedisString<List<FarmingItem>>(_redisConn, key, _dungeonExpiry);
            if (await redis.SetAsync(itemList) == false)
            {
                return ErrorCode.SetFarmingItemListFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.SetFarmingItemListFailException;
        }
    }

    public async Task<ErrorCode> SetKillNPCList(string email, List<KillNPC> NPCList)
    {
        var key = KeyMaker.MakeKillNPCKey(email);

        try
        {
            var redis = new RedisString<List<KillNPC>>(_redisConn, key, _dungeonExpiry);
            if (await redis.SetAsync(NPCList) == false)
            {
                return ErrorCode.SetFarmingItemListFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.SetFarmingItemListFailException;
        }
    }

    public async Task<ErrorCode> DeleteDungeonInfo(string email)
    {
        var deleteKillNPCErrorCode = await DeleteKillNPCList(email);
        if (deleteKillNPCErrorCode != ErrorCode.None)
        {
            // TODO : delete fail error
            return deleteKillNPCErrorCode;
        }

        var deleteFarmingItemErrorCode = await DeleteFarmingItemList(email);
        if (deleteFarmingItemErrorCode != ErrorCode.None)
        {
            // TODO : delete fail error
            return deleteFarmingItemErrorCode;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> DeleteKillNPCList(string email)
    {
        var key = KeyMaker.MakeKillNPCKey(email);

        try
        {
            var redis = new RedisString<List<KillNPC>>(_redisConn, key, _dungeonExpiry);
            if (await redis.DeleteAsync() == false)
            {
                return ErrorCode.DeleteKillNPCListFailNotExist;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.DeleteKillNPCListFailException;
        }
    }

    async Task<ErrorCode> DeleteFarmingItemList(string email)
    {
        var key = KeyMaker.MakeFarmingItemKey(email);

        try
        {
            var redis = new RedisString<List<FarmingItem>>(_redisConn, key, _dungeonExpiry);
            if (await redis.DeleteAsync() == false)
            {
                return ErrorCode.DeleteFarmingItemListNotExist;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.DeleteFarmingItemListFailException;
        }
    }
}

