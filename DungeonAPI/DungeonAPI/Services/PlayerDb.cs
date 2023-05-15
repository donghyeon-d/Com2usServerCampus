using System;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using DungeonAPI.ModelsDB;
using static Humanizer.In;

namespace DungeonAPI.Services;

public class PlayerDb : GameDb, IPlayerDb
{
    readonly ILogger<PlayerDb> _logger;

    public PlayerDb(ILogger<PlayerDb> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
    {
        _logger = logger;
    }

    public async Task<Tuple<ErrorCode, Int32>> CreatePlayerAsync(Int32 accountId)
    {
        _logger.LogDebug($"Where: Player.CreatePlayerAsync, Status: Try");
        // 값 넣기
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
            _logger.LogDebug($"Where: Player.LoadPlayerAsync, Status: Complete");
            return new(ErrorCode.None, characterId);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                $"Where: Player.LoadPlayerAsync, Status: {ErrorCode.CreatePlayerFailException}");
            return new(ErrorCode.CreatePlayerFailException, -1);
        }
    }

    public async Task<Tuple<ErrorCode, Player?>> LoadPlayerByPlayerIdAsync(Int32 playerId)
    {
        _logger.LogDebug($"Where: Player.LoadPlayerAsync, Status: Try");
        try
        {
            var playerInfo = await _queryFactory.Query("Player")
                .Where("PlayerId", playerId)
                .FirstOrDefaultAsync<Player>();
            if (playerInfo is null)
            {
                _logger.LogError($"Where: Player.LoadPlayerAsync, Status: {ErrorCode.PlayerNotExist}");
                return new(ErrorCode.PlayerNotExist, null);
            }
            _logger.LogDebug($"Where: Player.LoadPlayerAsync, Status: Complete");
            return new(ErrorCode.PlayerNotExist, playerInfo);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                $"Where: Player.LoadPlayerAsync, Status: {ErrorCode.LoadPlayerFailException}");
            return new(ErrorCode.CreatePlayerFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, Player?>> LoadPlayerByAccountAsync(Int32 accountId)
    {
        _logger.LogDebug($"Where: Player.LoadPlayerAsync, Status: Try");
        try
        {
            var playerInfo = await _queryFactory.Query("Player")
                .Where("AccountId", accountId)
                .FirstOrDefaultAsync<Player>();

            if (playerInfo is null)
            {
                _logger.LogError($"Where: Player.LoadPlayerAsync, Status: {ErrorCode.PlayerNotExist}");
                return new(ErrorCode.PlayerNotExist, null);
            }
            _logger.LogDebug($"Where: Player.LoadPlayerAsync, Status: Complete");
            return new(ErrorCode.None, playerInfo);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                $"Where: Player.LoadPlayerAsync, Status: {ErrorCode.LoadPlayerFailException}");
            return new(ErrorCode.LoadPlayerFailException, null);
        }
    }

    //public async Task<ErrorCode> AddMoney(Int32 playerId, Int32 moneyAmount)
    //{
    //    try
    //    {
    //        var hadMoney = await _queryFactory.Query("Player")
    //            .Where("PlayerId", playerId)
    //            .Select("Money")
    //            .FirstOrDefaultAsync<Int32>();
    //        if (hadMoney == 0)
    //        {
    //            return ErrorCode.AddMoneyNotFoundPlayer;
    //        }

    //        hadMoney += money;
    //        if (hadMoney < 0)
    //        {
    //            return ErrorCode.AddMoneyFailInvalidRange;
    //        }

    //        var count = await _queryFactory.Query("Player")
    //                                .Where("PlayerId", playerId)
    //                                .UpdateAsync(new { Money = hadMoney });
    //        if (count != 1)
    //        {
    //            return ErrorCode.AddMoneyFailUpdateFail;
    //        }

    //        return ErrorCode.None;
    //    }
    //    catch(Exception e)
    //    {
    //        return ErrorCode.AddMoneyFailFailException;
    //    }
    //}

    public async Task<ErrorCode> UpdatePlayerAsync(Player player)
    {
        _logger.LogDebug($"Where: Player.UpdatePlayerAsync, Status: Try");
        try
        {
            var affected = await _queryFactory.Query("Player")
                .Where("PlayerId", player.PlayerId)
                .UpdateAsync(player);
            // TODO : affected 리턴값 확인
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                $"Where: Player.LoadPlayerAsync, Status: {ErrorCode.UpdatePlayerFailException}");
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
            // TODO : log
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
                // TODO : log
                return new Tuple<ErrorCode, Int32>(ErrorCode.PlayerNotExist, -1);
            }
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, playerInfo.PlayerId);
        }
        catch (Exception e)
        {
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
                // TODO : log
                return ErrorCode.AddMoneyNotFoundPlayer;
            }

            return ErrorCode.None;
        }
        catch (Exception e)
        {
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
                // TODO : log
                return ErrorCode.AddExpNotFoundPlayer;
            }

            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.AddExpFailFailException;
        }
    }
}

