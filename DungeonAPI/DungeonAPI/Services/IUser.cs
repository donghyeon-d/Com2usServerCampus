using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface IUser
{
    public Task<Tuple<ErrorCode, Int32>> CreateUserAsync(Int32 accountId);

    public Task<Tuple<ErrorCode, UserSpec>> LoadUserAsync(Int32 UserId);

    public Task<Tuple<ErrorCode, UserSpec>> LoadUserByAccountAsync(Int32 accountId);

    public Task<ErrorCode> UpdateUserAsync(UserSpec user);

    public Task<ErrorCode> DeleteUserAsync(Int32 accountId);
}

