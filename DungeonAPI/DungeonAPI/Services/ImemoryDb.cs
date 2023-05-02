using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface ImemoryDb
{
    public Task<ErrorCode> CreateAuthUserAsync(string email, string authToken);

    public Task<ErrorCode> CheckUserAuthAsync(string email, string authToken);

    //public Task<ErrorCode> DeleteUserAuthAsync(string email, string authToken);

    //public Task<(bool, AuthUser)> GetUserAsync(string id);
}

