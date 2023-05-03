using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface IPlayerDb
{
    public Task<Tuple<ErrorCode, Int32>> CreatePlayerAsync(Int32 accountId);

    public Task<Tuple<ErrorCode, Player>> LoadPlayerAsync(Int32 playerId);

    public Task<Tuple<ErrorCode, Player>> LoadPlayerByAccountAsync(Int32 accountId);

    public Task<ErrorCode> UpdatePlayerAsync(Player player);

    public Task<ErrorCode> DeletePlayerAsync(Int32 accountId);
}

