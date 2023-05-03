using System;
using DungeonAPI.ModelDB;
using SqlKata.Execution;

namespace DungeonAPI.Services;

public interface IPlayerDb
{
    public Task<Tuple<ErrorCode, Int32>> CreatePlayerAsync(Int32 accountId);

    public Task<Tuple<ErrorCode, Player>> LoadPlayerByPlayerIdAsync(Int32 playerId);

    public Task<Tuple<ErrorCode, Player>> LoadPlayerByAccountAsync(Int32 accountId);

    public Task<ErrorCode> UpdatePlayerAsync(Player player);

    public Task<ErrorCode> DeletePlayerAsync(Int32 accountId);

    public Task<Tuple<ErrorCode, Int32>> LoadPlayerIdByAccountId(Int32 accountId);

    //public Task<Tuple<ErrorCode, Int32>> LoadPlayerIdByEmail(Int32 accountId);
}

