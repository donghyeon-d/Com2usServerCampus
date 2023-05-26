using System;
namespace DungeonAPI.ModelDB;

public class AttendanceBook
{
    public Int32 PlayerId { get; set; }
    public DateTime LastReceiveDate { get; set; }
    public Int32 DayCount { get; set; }

    public static AttendanceBook InitAttendanceBook(Int32 playerId)
    {
        return new AttendanceBook() { PlayerId = playerId };
    }

}

