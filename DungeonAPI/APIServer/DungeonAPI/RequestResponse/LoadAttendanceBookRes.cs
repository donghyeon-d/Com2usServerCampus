namespace DungeonAPI.RequestResponse;

public class LoadAttendanceBookRes : AuthPlayerResponse
{
	public Int32? DayCount { get; set; } = null;
	public bool? CanReceive { get; set; } = null;
}
