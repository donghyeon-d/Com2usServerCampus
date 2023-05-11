using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface IAttendanceBookDb
{
    public Task<Tuple<ErrorCode, AttendanceBook?>> LoadAttandanceBookInfo(Int32 playerId);
    public Task<ErrorCode> UpdateAttendanceBook(AttendanceBook attendanceBook);
    public Task<ErrorCode> CreatePlayerAttendanceBook(Int32 playerId);
    public Task<ErrorCode> DeletePlayerAttendanceBook(Int32 playerId);
}

