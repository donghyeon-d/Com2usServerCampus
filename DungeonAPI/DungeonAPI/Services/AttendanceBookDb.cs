using System;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using DungeonAPI.ModelsDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;

namespace DungeonAPI.Services;

public class AttendanceBookDb : GameDb, IAttendanceBookDb
{
    readonly ILogger<AttendanceBookDb> _logger;

    public AttendanceBookDb(ILogger<AttendanceBookDb> logger, IOptions<DbConfig> dbConfig,
        IMasterDataDb masterDataDb)
        : base(logger, dbConfig)
    {
        _logger = logger;
    }

    public async Task<Tuple<ErrorCode, AttendanceBook?>> LoadAttandanceBookInfo(Int32 playerId)
    {
        Open();
        try
        {
            var result = await _queryFactory.Query("AttendanceBook")
                                           .Where("PlayerId", playerId)
                                           .FirstOrDefaultAsync<AttendanceBook>();
            if (result is null)
            {
                return new (ErrorCode.LoadPlayerAttendanceBookNotExist, null);
            }

            return new (ErrorCode.None, result);
        }
        catch (Exception e)
        {
            return new (ErrorCode.LoadPlayerAttendanceBookFailException, null);
        }
    }

    
    public async Task<ErrorCode> UpdateAttendanceBook(AttendanceBook attendanceBook)
    {
        try
        {
            int updateResult = await _queryFactory.Query("AttendanceBook")
                                    .Where("PlayerId", attendanceBook.PlayerId)
                                    .UpdateAsync(new {
                                    LastReceiveDate = attendanceBook.LastReceiveDate,
                                    DayCount = attendanceBook.DayCount
                                    });
            if (updateResult != 1)
            {
                return ErrorCode.UpdateAttendanceBookTupleFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.UpdateAttendanceBookTupleFailException;
        }
    }

    //계정 생성하고 기본 데이터 생성할 때 생성하기
    public async Task<ErrorCode> CreatePlayerAttendanceBook(Int32 playerId)
    {
        Open();
        try
        {
            int count = await _queryFactory.Query("AttendanceBook").InsertAsync(new
            {
                PlayerId = playerId,
                StartDate = DateTime.Today,
                LastReceiveDate = DateTime.Today.AddDays(-10),
                DayCount = 0
            });

            if (count != 1)
            {
                return ErrorCode.CreatePlayerAttendanceBookFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.CreatePlayerAttendanceBookFailException;
        }
    }

    public async Task<ErrorCode> DeletePlayerAttendanceBook(Int32 playerId)
    {
        Open();
        try
        {
            int count = await _queryFactory.Query("AttendanceBook")
                                            .Where("PlayerId", playerId)
                                            .DeleteAsync();
            if (count != 1)
            {
                return ErrorCode.DeletePlayerAttendanceBookFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            // TODO : log
            return ErrorCode.DeletePlayerAttendanceBookFailException;
        }
    }
}

