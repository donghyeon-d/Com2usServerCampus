using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata;
using SqlKata.Execution;

namespace DungeonAPI.Services;

public class CompletedDungeonDb : GameDb, ICompletedDungeonDb
{
    readonly ILogger<CompletedDungeonDb> _logger;

    public CompletedDungeonDb(ILogger<CompletedDungeonDb> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
    {
        _logger = logger;
    }

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
            return new(ErrorCode.ReadCompleteListFailException, null);
        }
    }
}
