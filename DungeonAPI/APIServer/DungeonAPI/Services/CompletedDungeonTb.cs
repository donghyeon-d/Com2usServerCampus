using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata;
using SqlKata.Execution;
using ZLogger;

namespace DungeonAPI.Services;

public partial class GameDb : IGameDb
{
    public async Task<ErrorCode> CreatePlayerCompletedDungeon(Int32 playerId)
    {
        try
        {
            int result = await _queryFactory.Query("CompletedDungeon")
                              .InsertAsync(new
                              {
                                  PlayerId = playerId,
                              });
            if (result != 1)
            {
                return ErrorCode.CreatePlayerCompletedDungeonFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.CreatePlayerCompletedDungeonFailException;
        }
    }

    public async Task<ErrorCode> UpdateCompleteDungeon(Int32 playerId, Int32 stageCode)
    {
        try
        {
            int result = 0;
            if (IsForestThema(stageCode))
            {
                result = await _queryFactory.Query("CompletedDungeon")
                                   .Where("PlayerId", playerId)
                                   .UpdateAsync(new
                                   {
                                       ForestThema = stageCode
                                   });

            }
            else if (IsBeachThema(stageCode))
            {
                result = await _queryFactory.Query("CompletedDungeon")
                                   .Where("PlayerId", playerId)
                                   .UpdateAsync(new
                                   {
                                       BeachThema = stageCode
                                   });
            }
            else if (IsDesertThema(stageCode))
            {
                result = await _queryFactory.Query("CompletedDungeon")
                                   .Where("PlayerId", playerId)
                                   .UpdateAsync(new
                                   {
                                       DesertThema = stageCode
                                   });
            }
            else
                return ErrorCode.InvalidStageCode;

            if (result != 1)
            {
                return ErrorCode.UpdateCompletedDungeonFail;
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
            return ErrorCode.UpdateCompletedDungeonFailException;
        }
    }

    public async Task<Tuple<ErrorCode, CompletedDungeon?>> ReadCompleteList(Int32 playerId)
    {
        try
        {
            var result = await _queryFactory.Query("CompletedDungeon")
                                            .Where("PlayerId", playerId)
                                            .FirstOrDefaultAsync<CompletedDungeon>();
            if (result is null)
            {
                return new(ErrorCode.ReadCompleteListFail, null);
            }
            return new(ErrorCode.None, result);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new(ErrorCode.ReadCompleteListFailException, null);
        }
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

    //public async Task<ErrorCode> RollbackWhenUpdateFail(Int32 playerId, Int32 stageCode)
    //{
    //    try
    //    {
    //        int result = 0;
    //        if (IsForestThema(stageCode))
    //        {
    //            int rollbackSt
    //            result = await _queryFactory.Query("CompletedDungeon")
    //                               .Where("PlayerId", playerId)
    //                               .UpdateAsync(new
    //                               {
    //                                   ForestThema = stageCode
    //                               });

    //        }
    //        else if (IsBeachThema(stageCode))
    //        {
    //            result = await _queryFactory.Query("CompletedDungeon")
    //                               .Where("PlayerId", playerId)
    //                               .UpdateAsync(new
    //                               {
    //                                   BeachThema = stageCode
    //                               });
    //        }
    //        else if (IsDesertThema(stageCode))
    //        {
    //            result = await _queryFactory.Query("CompletedDungeon")
    //                               .Where("PlayerId", playerId)
    //                               .UpdateAsync(new
    //                               {
    //                                   DesertThema = stageCode
    //                               });
    //        }
    //        else
    //            return ErrorCode.InvalidStageCode;

    //        if (result != 1)
    //        {
    //            return ErrorCode.UpdateCompletedDungeonFail;
    //        }
    //        return ErrorCode.None;
    //    }
    //    catch (Exception e)
    //    {
    //        if (e.Message == "Duplicate entry '2-101' for key 'PRIMARY'")
    //        {
    //            return ErrorCode.None;
    //        }
    //        _logger.ZLogWarning(e.Message);
    //        return ErrorCode.UpdateCompletedDungeonFailException;
    //    }

    //}

    //public async Task<Tuple<ErrorCode, List<CompletedDungeon>?>> ReadCompleteList(Int32 playerId)
    //{
    //    try
    //    {
    //        var result = await _queryFactory.Query("CompletedDungeon")
    //                                        .Where("PlayerId", playerId)
    //                                        .GetAsync<CompletedDungeon>();
    //        if (result is null)
    //        {
    //            return new(ErrorCode.ReadCompleteListFail, null);
    //        }
    //        return new(ErrorCode.None, result.ToList());
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.ZLogWarning(e.Message);
    //        return new(ErrorCode.ReadCompleteListFailException, null);
    //    }
    //}

    public async Task<ErrorCode> CheckCanEnterStage(Int32 playerId, Int32 stageCode)
    {
        var requestStageInfo = MasterDataDb.s_stage.Find(stage => stage.StageCode == stageCode);
        if (requestStageInfo is null)
        {
            return ErrorCode.InvalidStageCode;
        }

        var (readCompleteThemaListErrorCode, completeStage) = await ReadCompleteList(playerId);
        if (readCompleteThemaListErrorCode != ErrorCode.None || completeStage is null)
        {
            return readCompleteThemaListErrorCode;
        }

        if (IsForestThema(stageCode))
        {
            return CanEnterRequestStage(stageCode, completeStage.ForestThema);
        }
        else if (IsBeachThema(stageCode))
        {
            return CanEnterRequestStage(stageCode, completeStage.BeachThema);
        }
        else if (IsDesertThema(stageCode))
        {
            return CanEnterRequestStage(stageCode, completeStage.DesertThema);
        }
        else
        {
            return ErrorCode.InvalidStageCode;
        }
    }

    ErrorCode CanEnterRequestStage(Int32 stageCode, Int32 CompleteStage)
    {
        if (CompleteStage == (int)Enum.StageCode.Empty &&
            (stageCode == (int)Enum.StageCode.ForestStart ||
            stageCode == (int)Enum.StageCode.BeachStart ||
            stageCode == (int)Enum.StageCode.DesertStart))
        {
            return ErrorCode.None;
        }
        if (stageCode <= CompleteStage + 1)
        {
            return ErrorCode.None;
        }
        else
        { 
            return ErrorCode.NeedToCompleteBeforeStage; 
        }
    }

    bool IsForestThema(Int32 stageCode)
    {
        if ((int)Enum.StageCode.ForestStart <= stageCode && 
            stageCode <= (int)Enum.StageCode.ForestEnd)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool IsBeachThema(Int32 stageCode)
    {
        if ((int)Enum.StageCode.BeachStart <= stageCode &&
            stageCode <= (int)Enum.StageCode.BeachEnd)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool IsDesertThema(Int32 stageCode)
    {
        if ((int)Enum.StageCode.DesertStart <= stageCode &&
            stageCode <= (int)Enum.StageCode.DesertEnd)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
