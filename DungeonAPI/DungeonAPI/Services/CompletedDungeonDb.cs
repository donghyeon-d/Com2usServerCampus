using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata;
using SqlKata.Execution;
using ZLogger;

namespace DungeonAPI.Services;

public partial class GameDb : IGameDb
{
    public async Task<ErrorCode> AddCompletedDungeon(Int32 playerId, Int32 stageCode)
    {
        try
        {
            int result = await _queryFactory.Query("CompletedDungeon")
                              .InsertAsync(new
                              {
                                  PlayerId = playerId,
                                  StageCode = stageCode
                              });
            if (result != 1)
            {
                return ErrorCode.AddCompletedDungeonFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            if (e.Message == "Duplicate entry '2-101' for key 'PRIMARY'")
            {
                return ErrorCode.None;
            }
            _logger.ZLogWarning(e.Message);
            return ErrorCode.AddCompletedDungeonFailException;
        }
    }

    public async Task<ErrorCode> DeleteWhenFail(Int32 playerId, Int32 stageCode)
    {
        try
        {
            int result = await _queryFactory.Query("CompletedDungeon")
                                            .Where("PlayerId", playerId)
                                            .Where("StageCode", stageCode)
                                            .DeleteAsync();
            if (result != 1)
            {
                return ErrorCode.DeleteCompletedDungeonFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.DeleteCompletedDungeonFailException;
        }
    }

    public async Task<Tuple<ErrorCode, List<CompletedDungeon>?>> ReadCompleteList(Int32 playerId)
    {
        try
        {
            var result = await _queryFactory.Query("CompletedDungeon")
                                            .Where("PlayerId", playerId)
                                            .GetAsync<CompletedDungeon>();
            if (result is null)
            {
                return new(ErrorCode.ReadCompleteListFail, null);
            }
            return new(ErrorCode.None, result.ToList());
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new(ErrorCode.ReadCompleteListFailException, null);
        }
    }
}
