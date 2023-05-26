using Microsoft.AspNetCore.Mvc;
using DungeonAPI.Services;
using DungeonAPI.RequestResponse;
using DungeonAPI.ModelDB;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoadAttendanceBookController : ControllerBase
{
	readonly ILogger<LoadAttendanceBookController> _logger;
	readonly IGameDb _gameDb;

	public LoadAttendanceBookController(ILogger<LoadAttendanceBookController> logger,
        IGameDb gameDb)
	{
		_logger = logger;
		_gameDb = gameDb;
    }

    [HttpPost]
    public async Task<LoadAttendanceBookRes> LoadAttendanceBook(LoadAttendanceBookReq request)
	{
        var player = (PlayerInfo)HttpContext.Items["PlayerInfo"];

        LoadAttendanceBookRes response = new();

        var (loadAttandanceBookErrorCode, attendanceBook) = await _gameDb.LoadAttandanceBookInfo(player.Id);
		if (loadAttandanceBookErrorCode != ErrorCode.None || attendanceBook is null)
		{
			response.Result = loadAttandanceBookErrorCode;
            _logger.ZLogInformationWithPayload(new { Player = player.Id }, response.Result.ToString());
            return response;
		}

        response.CanReceive = CanReceiveAttendanceReward(attendanceBook);
        response.DayCount = GetDayCount(attendanceBook);
        _logger.ZLogInformationWithPayload(new { Player = player.Id, DayCount = response.DayCount, CanReceive = response.CanReceive }, response.Result.ToString());
        return response;
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

