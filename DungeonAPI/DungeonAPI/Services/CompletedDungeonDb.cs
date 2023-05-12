using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
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

    public async Task<ErrorCode> AddCompletedDungeon(Int32 playerId, String thema, Int32 step)
    {
        try
        {
            int result = await _queryFactory.Query("CompletedDungeon")
                              .InsertAsync(new { 
                                  PlayerId = playerId, 
                                  Thema = thema,
                                  Step = step });
            if (result != 1)
            {
                return ErrorCode.AddCompletedDungeonFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.AddCompletedDungeonFailException;
        }
    }

    public async Task<ErrorCode> DeleteWhenFail(Int32 playerId, String thema, Int32 step)
    {
        try
        {
            int result = await _queryFactory.Query("CompletedDungeon")
                                            .Where("PlayerId", playerId)
                                            .Where("Thema", thema)
                                            .Where("Step", step)
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
                return new(ErrorCode.CompleteListNotExist, null);
            }
            return new(ErrorCode.None, result.ToList());
        }
        catch (Exception e)
        {
            return new(ErrorCode.ReadCompleteListFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, List<CompletedDungeon>?>> ReadCompleteThemaList(Int32 playerId, String thema)
    {
        try
        {
            var result = await _queryFactory.Query("CompletedDungeon")
                                            .Where("PlayerId", playerId)
                                            .Where("Thema", thema)
                                            .GetAsync<CompletedDungeon>();
            if (result is null)
            {
                return new(ErrorCode.CompleteThemaListNotExist, null);
            }
            return new(ErrorCode.None, result.ToList());
        }
        catch (Exception e)
        {
            return new(ErrorCode.ReadCompleteThemaListFailException, null);
        }
    }
}
