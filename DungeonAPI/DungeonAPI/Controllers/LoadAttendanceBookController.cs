using Microsoft.AspNetCore.Mvc;
using DungeonAPI.Services;
using DungeonAPI.RequestResponse;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoadAttendanceBookController : ControllerBase
{
	readonly ILogger<LoadAttendanceBookController> _logger;
	readonly IAttendanceBookDb _attendanceBookDb;

	public LoadAttendanceBookController(ILogger<LoadAttendanceBookController> logger,
        IAttendanceBookDb attendanceBookDb)
	{
		_logger = logger;
		_attendanceBookDb = attendanceBookDb;
    }

    [HttpPost]
    public async Task<LoadAttendanceBookRes> LoadAttendanceBook()
	{
        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        LoadAttendanceBookRes respons = new();

        var (loadAttandanceBookErrorCode, attendanceBook) = await _attendanceBookDb.LoadAttandanceBookInfo(playerId);
		if (loadAttandanceBookErrorCode != ErrorCode.None || attendanceBook is null)
		{
			respons.Result = loadAttandanceBookErrorCode;
			return respons;
		}

        respons.CanReceive = CanReceiveAttendanceReward(attendanceBook);
        respons.DayCount = GetDayCount(attendanceBook);
        return respons;
    }
    
    bool CanReceiveAttendanceReward(AttendanceBook attendanceBook)
    {
        if (attendanceBook.LastReceiveDate.Date == DateTime.Today.Date)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    int GetDayCount(AttendanceBook attendanceBook)
    {
        if (attendanceBook.LastReceiveDate.Date == DateTime.Today.Date)
        {
            return attendanceBook.DayCount;
        }
        else if (attendanceBook.LastReceiveDate.Date == DateTime.Today.AddDays(-1).Date) 
        {
            if (attendanceBook.DayCount == 30)
            {
                return 1;
            }
            else
            {
                return attendanceBook.DayCount + 1;
            }
        }
        else
        {
            return 1;
        }
    }
}

