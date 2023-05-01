using System;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using System.Data;
using MySqlConnector;
using DungeonAPI.ModelsDB;
using Microsoft.Extensions.Logging;

namespace DungeonAPI.Services;

public class User : GameDb, IUser
{
    readonly ILogger<User> _logger;

    public User(ILogger<User> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
    {
        _logger = logger;
    }

    public async Task<Tuple<ErrorCode, Int32>> CreateUserAsync(Int32 accountId)
    {
        _logger.LogDebug($"Where: User.CreateUserAsync, Status: Try");
        // 값 넣기
        try
        {
            var characterId = await _queryFactory.Query("User").InsertGetIdAsync<int>(new
            {
                AccountId = accountId,
                Exp = 100,
                Level = 1,
                Hp = 100,
                Mp = 100,
                Attack = 10,
                Defence = 10,
                Magic = 10
            });
            _logger.LogDebug($"Where: User.LoadUserAsync, Status: Complete");
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, characterId);
        }
        catch(Exception e)
        {
            _logger.LogError(e,
                $"Where: User.LoadUserAsync, Status: {ErrorCode.UserCreateFailException}");
            return new Tuple<ErrorCode, Int32>(ErrorCode.UserCreateFailException, -1);
        }
        finally
        {
            Dispose();
        }
    }

    public async Task<Tuple<ErrorCode, ModelDB.User>> LoadUserAsync(Int32 userId)
    {
        _logger.LogDebug($"Where: User.LoadUserAsync, Status: Try");
        try
        {
            var UserInfo = await _queryFactory.Query("User")
                .Where("UserId", userId)
                .FirstOrDefaultAsync<ModelDB.User>();
            if (UserInfo is null)
            {
                _logger.LogError($"Where: User.LoadUserAsync, Status: {ErrorCode.UserNotExist}");
                return new Tuple<ErrorCode, ModelDB.User>(ErrorCode.UserNotExist, null);
            }
            _logger.LogDebug($"Where: User.LoadUserAsync, Status: Complete");
            return new Tuple<ErrorCode, ModelDB.User>(ErrorCode.UserNotExist, UserInfo);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                $"Where: User.LoadUserAsync, Status: {ErrorCode.UserLoadFailException}");
            return new Tuple<ErrorCode, ModelDB.User>(ErrorCode.UserCreateFailException, null);
        }
        finally
        {
            Dispose();
        }
    }

    public async Task<ErrorCode> UpdateUserAsync(ModelDB.User user)
    {
        _logger.LogDebug($"Where: User.UpdateUserAsync, Status: Try");
        try
        {
            var affected = await _queryFactory.Query("User")
                .Where("UserId", user.UserId)
                .UpdateAsync(user);
            // TODO : affected 리턴값 확인
            return ErrorCode.None;
        }
        catch ( Exception e )
        {
            _logger.LogError(e,
                $"Where: User.LoadUserAsync, Status: {ErrorCode.UserUpdateFailException}");
            return ErrorCode.UserUpdateFailException;
        }
        finally
        {
            Dispose();
        }
    }
}

