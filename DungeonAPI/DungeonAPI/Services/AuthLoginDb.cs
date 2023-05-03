using System;
using CloudStructures;
using CloudStructures.Structures;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using DungeonAPI.ModelsDB;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;

namespace DungeonAPI.Services;

public class AuthLoginDb : IAuthLoginDb
{
    readonly RedisConnection _redisConn;
    readonly ILogger<AuthLoginDb> _logger;
    readonly IOptions<DbConfig> _dbConfig;

    public AuthLoginDb(ILogger<AuthLoginDb> logger, IOptions<DbConfig> dbConfig)
	{
        _logger = logger;
        _dbConfig = dbConfig;
        var redisAddress = _dbConfig.Value.Redis;
        var redisConfig = new RedisConfig("default", redisAddress);
        _redisConn = new RedisConnection(redisConfig);

        // TODO: log
    }

    

    public async Task<ErrorCode> CreateAuthPlayerAsync(string email, string authToken)
    {
        var key = email;
        var defaultExpiry = TimeSpan.FromDays(1); // TODO: Expiry setting
        try
        {
            var redis = new RedisString<AuthPlayer>(_redisConn, key, defaultExpiry);
            AuthPlayer value = new AuthPlayer { AuthToken = authToken };
            try
            {
                await redis.SetAsync(value);
                return ErrorCode.None;
            }
            catch
            {
                return ErrorCode.AuthTockenCreateFailException;
            }
        }
        catch
        {
            return ErrorCode.AuthTockenFailException;
        }
    }

    public async Task<ErrorCode> CheckPlayerAuthAsync(string email, string authToken)
    {
        var key = email;
        var defaultExpiry = TimeSpan.FromDays(1);

        try
        {
            var redis = new RedisString<AuthPlayer>(_redisConn, key, defaultExpiry);
            try
            {
                var value = await redis.GetAsync();
                if (value.Value.AuthToken != authToken)
                {
                    return ErrorCode.AuthTokenMismatch;
                }
                return ErrorCode.None;

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message); //TODO:
                return ErrorCode.AuthTokenNotFound;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message); // TODO:
            return ErrorCode.AuthTockenFailException;
        }
    }

    public async Task<ErrorCode> DeletePlayerAuthAsync(string email)
    {
        var key = email;
        var defaultExpiry = TimeSpan.FromDays(1);
        var redis = new RedisString<AuthPlayer>(_redisConn, key, defaultExpiry);

        if (await redis.DeleteAsync() == false)
        {
            return ErrorCode.DeleteAccountFail;
        }
        return ErrorCode.None;
    }
}

