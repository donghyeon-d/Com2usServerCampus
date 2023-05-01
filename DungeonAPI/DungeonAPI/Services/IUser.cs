using System;
namespace DungeonAPI.Services;

public interface IUser
{
    public Task<Tuple<ErrorCode, Int32>> CreateUserAsync(Int32 accountId);

    public Task<Tuple<ErrorCode, ModelDB.User>> LoadUserAsync(Int32 UserId);

    public Task<ErrorCode> UpdateUserAsync(ModelDB.User user);

    //public Task<ErrorCode> DeleteUserAsync(String email, String pw);
}

