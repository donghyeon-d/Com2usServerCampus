using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IAuthLoginDb
{
    public Task<ErrorCode> CreateAuthPlayerAsync(string email, string authToken);

    public Task<ErrorCode> CheckPlayerAuthAsync(string email, string authToken);

    public Task<ErrorCode> DeletePlayerAuthAsync(string email);

    //public Task<(bool, AuthPlayer)> GetPlayerAsync(string id);
}

