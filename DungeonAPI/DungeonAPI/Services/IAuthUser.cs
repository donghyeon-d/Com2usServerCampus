using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IAuthUserDb
{
    public Task<ErrorCode> CreateAuthUserAsync(string email, string authToken, Int32 playerId);

    public Task<ErrorCode> CheckAuthUserAsync(string email, string authToken);

    public Task<ErrorCode> DeleteAuthUserAsync(string email);

    public Task<Tuple<ErrorCode, AuthUser>> LoadAuthUserByEmail(string email);
}

