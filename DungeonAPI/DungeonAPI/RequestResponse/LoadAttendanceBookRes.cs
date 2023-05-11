using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class LoadAttendanceBookRes
{
	public ErrorCode Result { get; set; } = ErrorCode.None;
	public Int32 DayCount { get; set; }
    public bool CanReceive { get; set; }
}
