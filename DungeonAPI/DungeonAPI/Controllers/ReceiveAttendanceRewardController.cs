using System;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReceiveAttendanceRewardController : ControllerBase
{
	readonly ILogger<ReceiveAttendanceRewardController> _logger;
	readonly IAttendanceBookDb _attendanceBookDb;
    readonly IMailDb _mailDb;

    public ReceiveAttendanceRewardController(ILogger<ReceiveAttendanceRewardController> logger,
        IAttendanceBookDb attendanceBookDb, IMailDb mailDb)
	{
		_logger = logger;
		_attendanceBookDb = attendanceBookDb;
        _mailDb = mailDb;
    }

    [HttpPost]
    public async Task<ReceiveAttendanceRewardRes> LoadAttendanceBook()
	{
		ReceiveAttendanceRewardRes response = new ReceiveAttendanceRewardRes();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        response.Result = await ReceiveRewardToMail(playerId);
        return response;
    }

    async Task<ErrorCode> ReceiveRewardToMail(Int32 playerId)
    {
        var (loadAttandanceBookErrorCode, attendanceBook) 
                = await _attendanceBookDb.LoadAttandanceBookInfo(playerId);
        if (loadAttandanceBookErrorCode != ErrorCode.None || attendanceBook is null)
        {
            return loadAttandanceBookErrorCode;
        }

        if (attendanceBook.LastReceiveDate.Date == DateTime.Today.Date)
        {
            return ErrorCode.AlreadyReceiveAttendanceReward;
        }

        AttendanceBook todayAttendanceBook = CalcTodayAttendanceBook(attendanceBook);
        
        var updateAttendanceErrorCode = await _attendanceBookDb.UpdateAttendanceBook(todayAttendanceBook);
        if (updateAttendanceErrorCode != ErrorCode.None)
        {
            return updateAttendanceErrorCode;
        }

        var (sendMailErrorCode, mailId) = await SendToMailDailyReward(playerId, todayAttendanceBook.DayCount);
        if (sendMailErrorCode != ErrorCode.None)
        {
            var rollbackErrorCode = await _attendanceBookDb.UpdateAttendanceBook(attendanceBook);
            if (rollbackErrorCode != ErrorCode.None)
            {
                // TODO : RollbackError
            }
            return sendMailErrorCode;
        }

        return ErrorCode.None;
    }

    AttendanceBook CalcTodayAttendanceBook(AttendanceBook attendanceBook)
    {
        AttendanceBook UpdateAttendanceBook = new () { PlayerId = attendanceBook.PlayerId};

        UpdateAttendanceBook.LastReceiveDate = DateTime.Today;
        
        if (attendanceBook.LastReceiveDate.Date == DateTime.Today.AddDays(-1).Date &&
            attendanceBook.DayCount != 30)
        {
            UpdateAttendanceBook.DayCount = attendanceBook.DayCount + 1;
        }
        else
        {
            UpdateAttendanceBook.DayCount = 1;
        }
        
        return UpdateAttendanceBook;
    }

    async Task<Tuple<ErrorCode, Int32>> SendToMailDailyReward(Int32 playerId, Int32 dayCount)
    {
        var reward = MasterDataDb.s_attendanceReward.Find(item => item.Day == dayCount);
        if (reward is null)
        {
            return new(ErrorCode.InvalidDayCount, -1);
        }

        Mail mail = new()
        {
            PlayerId = playerId,
            Title = "AttendanceBook Reward",
            Content = "This is AttendanceBook Reward! Enjoy you Game Life!",
            ExpiredDate = DateTime.Today.AddDays(14),
            Sender = "AttendanceBook",
            ItemCode = reward.Code,
            ItemCount = reward.Count
        };

        return await _mailDb.SendMail(mail);
    }
}

