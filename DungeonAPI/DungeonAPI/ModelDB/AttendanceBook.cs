using System;
namespace DungeonAPI.ModelDB;

public class AttendanceBook
{
    public Int32 PlayerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime LastReceiveDate { get; set; }
    public Int32 ConsecutiveDays { get; set; }
}

