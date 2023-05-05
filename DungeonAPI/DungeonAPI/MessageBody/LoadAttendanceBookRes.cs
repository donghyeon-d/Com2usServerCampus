using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.MessageBody;

public class LoadAttendanceBookRes
{
	public ErrorCode Result { get; set; } = ErrorCode.None;
	public Int32 ConsecutiveDays { get; set; }
    public bool CanReceive { get; set; }
    public List<MasterData.AttendanceReward> RewardList { get; set; }
}
