using CloudStructures;
using CloudStructures.Structures;
using DungeonAPI.Configs;
using DungeonAPI.Enum;
using DungeonAPI.ModelDB;
using DungeonAPI.Util;
using Microsoft.Extensions.Options;
using ZLogger;

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
    }

    public async Task<ErrorCode> CreatePlayerInfo(string email, string authToken, Int32 playerId)
    {

        var key = KeyMaker.MakePlayerInfoKey(email);
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
            _logger.ZLogWarning(e.Message);
            return ErrorCode.AuthTockenCreateFailException;
        }
    }

    public async Task<ErrorCode> ChangeUserStatus(string email, PlayerStatus status, Int32 stageCode = 0)
    {
        var key = KeyMaker.MakePlayerInfoKey(email);

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
            value.CurrentStage = stageCode;

            if (await redis.SetAsync(value) == false)
            {
                return ErrorCode.ChangUserSetStautsFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.ChangUserStatusFailException;
        }
    }

    public async Task<ErrorCode> UpdateUserStatus(string email, PlayerInfo value)
    {
        var key = KeyMaker.MakePlayerInfoKey(email);

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
            _logger.ZLogWarning(e.Message);
            return ErrorCode.ChangUserStatusFailException;
        }
    }

    public async Task<ErrorCode> DeletePlayer(string email)
    {
        var key = KeyMaker.MakePlayerInfoKey(email);

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
            _logger.ZLogWarning(e.Message);
            return ErrorCode.AuthTokenFailException;
        }
    }

    public async Task<Tuple<ErrorCode, PlayerInfo?>> LoadPlayer(string email)
    {
        var key = KeyMaker.MakePlayerInfoKey(email);

        try
        {
            var redis = new RedisString<PlayerInfo>(_redisConn, key, _defaultExpiry);
            var result = await redis.GetAsync();
            if (result.HasValue == false)
            {
                return new(ErrorCode.AuthTokenNotFound, null);
            }
            return new(ErrorCode.None, result.Value);

        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new(ErrorCode.AuthTokenFailException, null);
        }
    }

    public async Task<ErrorCode> DeleteDungeonInfo(string email)
    {
        var key = KeyMaker.MakeInDungeonKey(email);

        try
        {
            var redis = new RedisString<InDungeon>(_redisConn, key, _defaultExpiry);
            if (await redis.DeleteAsync() == false)
            {
                return ErrorCode.DeleteDungeonInfoFailNotExist;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.DeleteDungeonInfoFailException;
        }
    }
    public async Task<Tuple<ErrorCode, InDungeon?>> GetDungeonInfo(string email)
    {
        var key = KeyMaker.MakeInDungeonKey(email);

        try
        {
            var redis = new RedisString<InDungeon>(_redisConn, key, _defaultExpiry);
            var dungeonInfo = await redis.GetAsync();
            if (dungeonInfo.HasValue == false)
            {
                return new(ErrorCode.GetDungeonInfoFailNotExist, null);
            }
            return new(ErrorCode.None, dungeonInfo.Value);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new(ErrorCode.GetDungeonInfoFailException, null);
        }
    }

    public async Task<ErrorCode> SetDungeonInfo(string email, InDungeon dungeonInfo)
    {
        var key = KeyMaker.MakeInDungeonKey(email);

        try
        {
            var redis = new RedisString<InDungeon>(_redisConn, key, _defaultExpiry);
            if (await redis.SetAsync(dungeonInfo) == false)
            {
                return ErrorCode.SetDungeonInfoFailNotExist;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.SetDungeonInfoFailException;
        }
    }
}

