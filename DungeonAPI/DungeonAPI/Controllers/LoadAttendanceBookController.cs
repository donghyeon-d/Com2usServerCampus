using System;
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
	readonly IMasterDataDb _masterDataDb;

	public LoadAttendanceBookController(ILogger<LoadAttendanceBookController> logger,
        IAttendanceBookDb attendanceBookDb, IMasterDataDb masterDataDb)
	{
		_logger = logger;
		_attendanceBookDb = attendanceBookDb;
		_masterDataDb = masterDataDb;
    }

    [HttpPost]
    public async Task<LoadAttendanceBookRes> LoadAttendanceBook(LoadAttendanceBookReq request)
	{
        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        LoadAttendanceBookRes respons = new LoadAttendanceBookRes();

        var (loadAttandanceBookErrorCode, attendanceBook) = await _attendanceBookDb.LoadAttandanceBookInfoByPlayerId(playerId);
		if (loadAttandanceBookErrorCode != ErrorCode.None)
		{
			respons.Result = loadAttandanceBookErrorCode;
			return respons;
		}

        respons.CanReceive = CanReceiveAttendanceReward(attendanceBook);
        respons.ConsecutiveDays = GetConsecutiveDays(attendanceBook);
        respons.RewardList = MasterDataDb.s_attendanceReward;
        return respons;
    }

    Int32 GetConsecutiveDays(AttendanceBook attendanceBook)
    {
        if (attendanceBook.StartDate.Date == attendanceBook.LastReceiveDate.Date)
        {
            return attendanceBook.ConsecutiveDays;
        }
        if (attendanceBook.StartDate.Date == attendanceBook.LastReceiveDate.AddDays(-1).Date)
        {
            return attendanceBook.ConsecutiveDays + 1;
        }
        return 1;
    }

    bool CanReceiveAttendanceReward(AttendanceBook attendanceBook)
    {
        if (attendanceBook.StartDate.Date == attendanceBook.LastReceiveDate.Date)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}

