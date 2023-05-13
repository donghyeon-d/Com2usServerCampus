using System;
using DungeonAPI.ModelDB;
using SqlKata.Execution;

namespace DungeonAPI.Services;

public interface IPlayerDb
{
    public Task<Tuple<ErrorCode, Int32>> CreatePlayerAsync(Int32 accountId);

    public Task<Tuple<ErrorCode, Player?>> LoadPlayerByPlayerIdAsync(Int32 playerId);

    public Task<Tuple<ErrorCode, Player?>> LoadPlayerByAccountAsync(Int32 accountId);

    public Task<ErrorCode> UpdatePlayerAsync(Player player);

    public Task<ErrorCode> DeletePlayerAsync(Int32 accountId);

    public Task<Tuple<ErrorCode, Int32>> LoadPlayerIdByAccountIdAsync(Int32 accountId);

    public Task<ErrorCode> AddMoney(Int32 playerId, Int32 amount);
    public Task<ErrorCode> AddExp(Int32 playerId, Int32 amount);


    //public Task<Tuple<ErrorCode, Int32>> LoadPlayerIdByEmail(Int32 accountId);
}

