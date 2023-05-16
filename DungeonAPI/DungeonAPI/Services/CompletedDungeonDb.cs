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

    public async Task<ErrorCode> AddCompletedDungeon(Int32 playerId, String thema, Int32 stage)
    {
        try
        {

            var notIn = _queryFactory.Query("CompletedDungeon")
                            .WhereNotIn("PlayerId", q => q.From("CompletedDungeon")
                            .Where("PlayerId", playerId))
                            .WhereNotIn("Thema", q => q.From("CompletedDungeon")
                            .Where("Thema", thema))
                            .WhereNotIn("Stage", q => q.From("CompletedDungeon")
                            .Where("Stage", stage))
            .Select("PlayerId", "Thema", "Stage");
                            //.WhereNotIn("Thema", q => q.From("CompletedDungeon")
                            //    .Where("Thema", thema))
                            //.WhereNotIn("Stage", q => q.From("CompletedDungeon")
                            //    .Where("Stage", stage))
                            //.Select("playerId", "thema", "stage");
            //.FromRaw("( VALUES ( ?, ?, ? )) AS t ( PlayerId, Thema, Stage )", playerId, thema, stage)

            var insert = _queryFactory.Query("CompletedDungeon")
                .AsInsert(new[] { "PlayerId", "Thema", "Stage" }, notIn);



            var result = await _queryFactory.ExecuteAsync(insert);

            //int result = await _queryFactory.Query("CompletedDungeon")
            //                  .InsertAsync(new
            //                  {
            //                      PlayerId = playerId,
            //                      Thema = thema,
            //                      Stage = stage
            //                  });
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

    public async Task<ErrorCode> DeleteWhenFail(Int32 playerId, String thema, Int32 stage)
    {
        try
        {
            int result = await _queryFactory.Query("CompletedDungeon")
                                            .Where("PlayerId", playerId)
                                            .Where("Thema", thema)
                                            .Where("Stage", stage)
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
                return new(ErrorCode.ReadCompleteThemaListFail, null);
            }
            return new(ErrorCode.None, result.ToList());
        }
        catch (Exception e)
        {
            return new(ErrorCode.ReadCompleteThemaListFailException, null);
        }
    }
}
