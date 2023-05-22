using System;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using DungeonAPI.ModelsDB;
using static Humanizer.In;
using ZLogger;

namespace DungeonAPI.Services;

public partial class GameDb : IGameDb
{
    public async Task<Tuple<ErrorCode, Int32>> CreatePlayerAsync(Int32 accountId)
    {
        try
        {
            var characterId = await _queryFactory.Query("Player").InsertGetIdAsync<int>(new
            {
                AccountId = accountId,
                Exp = 100,
                Level = 1,
                Hp = 100,
                Mp = 100,
                Attack = 10,
                Defence = 10,
                Magic = 10,
                Money = 0
            });
            return new(ErrorCode.None, characterId);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new(ErrorCode.CreatePlayerFailException, -1);
        }
    }

    public async Task<Tuple<ErrorCode, Player?>> LoadPlayerByPlayerIdAsync(Int32 playerId)
    {
        try
        {
            var playerInfo = await _queryFactory.Query("Player")
                .Where("PlayerId", playerId)
                .FirstOrDefaultAsync<Player>();
            if (playerInfo is null)
            {
                return new(ErrorCode.PlayerNotExist, null);
            }
            return new(ErrorCode.PlayerNotExist, playerInfo);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new(ErrorCode.CreatePlayerFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, Player?>> LoadPlayerByAccountAsync(Int32 accountId)
    {
        try
        {
            var playerInfo = await _queryFactory.Query("Player")
                .Where("AccountId", accountId)
                .FirstOrDefaultAsync<Player>();

            if (playerInfo is null)
            {
                return new(ErrorCode.PlayerNotExist, null);
            }
            return new(ErrorCode.None, playerInfo);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new(ErrorCode.LoadPlayerFailException, null);
        }
    }


    public async Task<ErrorCode> UpdatePlayerAsync(Player player)
    {
        try
        {
            var affected = await _queryFactory.Query("Player")
                .Where("PlayerId", player.PlayerId)
                .UpdateAsync(player);
            if (affected != 1)
            {
                return ErrorCode.UpdatePlayerFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.UpdatePlayerFailException;
        }
    }

    public async Task<ErrorCode> DeletePlayerAsync(Int32 accountId)
    {
        Open();
        try
        {
            int count = await _queryFactory.Query("Player")
                                            .Where("AccountId", accountId)
                                            .DeleteAsync();
            if (count != 1)
            {
                return ErrorCode.DeletePlayerFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.DeletePlayerFailException;
        }
    }

    public async Task<Tuple<ErrorCode, Int32>> LoadPlayerIdByAccountIdAsync(Int32 accountId)
    {

        try
        {
            Open();
            var playerInfo = await _queryFactory.Query("Player")
                    .Where("AccountId", accountId)
                    .FirstOrDefaultAsync<Player>();
            if (playerInfo is null)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.PlayerNotExist, -1);
            }
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, playerInfo.PlayerId);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new Tuple<ErrorCode, Int32>(ErrorCode.LoadPlayerFailException, -1);
        }
    }

    public async Task<ErrorCode> AddMoney(Int32 playerId, Int32 amount)
    {
        try
        {
            var result = await _queryFactory.Query("Player")
                    .Where("PlayerId", playerId)
                    .IncrementAsync("Money", amount);
            if (result != 1)
            {
                return ErrorCode.AddMoneyNotFoundPlayer;
            }

            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.AddMoneyFailFailException;
        }
    }

    public async Task<ErrorCode> AddExp(Int32 playerId, Int32 amount)
    {
        try
        {
            var result = await _queryFactory.Query("Player")
                    .Where("PlayerId", playerId)
                    .IncrementAsync("Exp", amount);
            if (result != 1)
            {
                return ErrorCode.AddExpNotFoundPlayer;
            }

            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.AddExpFailFailException;
        }
    }
}

