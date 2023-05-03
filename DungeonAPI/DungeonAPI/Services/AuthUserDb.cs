using System;
using CloudStructures;
using CloudStructures.Structures;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using DungeonAPI.ModelsDB;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;

namespace DungeonAPI.Services;

public class AuthUserDb : IAuthUserDb
{
    readonly RedisConnection _redisConn;
    readonly ILogger<AuthUserDb> _logger;
    readonly IOptions<DbConfig> _dbConfig;
    TimeSpan _defaultExpiry = TimeSpan.FromDays(1);

    public AuthUserDb(ILogger<AuthUserDb> logger, IOptions<DbConfig> dbConfig)
	{
        _logger = logger;
        _dbConfig = dbConfig;
        var redisAddress = _dbConfig.Value.Redis;
        var redisConfig = new RedisConfig("default", redisAddress);
        _redisConn = new RedisConnection(redisConfig);

        // TODO: log
    }

    

    public async Task<ErrorCode> CreateAuthUserAsync(string email, string authToken, Int32 playerId)
    {

        var key = email;
         // TODO: Expiry setting
        try
        {
            var redis = new RedisString<AuthUser>(_redisConn, key, _defaultExpiry);
            AuthUser value = new AuthUser {
                                AuthToken = authToken,
                                PlayerId = playerId };
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

    public async Task<ErrorCode> CheckAuthUserAsync(string email, string authToken)
    {
        var (loadAuthUserErrorCode, authUser) = await LoadAuthUserByEmail(email);

        if (loadAuthUserErrorCode != ErrorCode.None)
        {
            return loadAuthUserErrorCode;
        }

        if (authUser.AuthToken != authToken)
        {
            return ErrorCode.AuthTokenMismatch;
        }

        return ErrorCode.None;
    }

    public async Task<ErrorCode> DeleteAuthUserAsync(string email)
    {
        try
        {
            var key = email;
            var redis = new RedisString<AuthUser>(_redisConn, key, _defaultExpiry);

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

    public async Task<Tuple<ErrorCode, AuthUser>> LoadAuthUserByEmail(string email)
    {
        var key = email;

        try
        {
            var redis = new RedisString<AuthUser>(_redisConn, key, _defaultExpiry);
            var result = await redis.GetAsync();
            if (result.HasValue == false)
            {
                return new Tuple<ErrorCode, AuthUser>(ErrorCode.AuthTokenNotFound, result.Value);
            }
            return new Tuple<ErrorCode, AuthUser>(ErrorCode.None, result.Value);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message); //TODO:
            return new Tuple<ErrorCode, AuthUser>(ErrorCode.AuthTokenFailException, null);
        }
    }
}

