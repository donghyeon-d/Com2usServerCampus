using System;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReceiveAttendanceRewardController : ControllerBase
{
	readonly ILogger<ReceiveAttendanceRewardController> _logger;
	readonly IGameDb _gameDb;

    public ReceiveAttendanceRewardController(ILogger<ReceiveAttendanceRewardController> logger,
        IGameDb attendanceBookDb)
	{
		_logger = logger;
		_gameDb = attendanceBookDb;
    }

    [HttpPost]
    public async Task<ReceiveAttendanceRewardRes> LoadAttendanceBook(ReceiveAttendanceRewardReq request)
	{
		ReceiveAttendanceRewardRes response = new ReceiveAttendanceRewardRes();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        var (receiveItemErrorCode, mailId) = await ReceiveItemToMail(playerId);
        response.Result = receiveItemErrorCode;

        _logger.ZLogInformationWithPayload(new { PlayerId = playerId, MailId = mailId }, response.Result.ToString());

        return response;
    }

    async Task<Tuple<ErrorCode, Int32>> ReceiveItemToMail(Int32 playerId)
    {
        var (loadAttandanceBookErrorCode, attendanceBook) 
                = await _gameDb.LoadAttandanceBookInfo(playerId);
        if (loadAttandanceBookErrorCode != ErrorCode.None || attendanceBook is null)
        {
            return new (loadAttandanceBookErrorCode, 0);
        }

        if (attendanceBook.LastReceiveDate.Date == DateTime.Today.Date)
        {
            return new (ErrorCode.AlreadyReceiveAttendanceReward, 0);
        }

        AttendanceBook todayAttendanceBook = CalcTodayAttendanceBook(attendanceBook);
        
        var updateAttendanceErrorCode = await _gameDb.UpdateAttendanceBook(todayAttendanceBook);
        if (updateAttendanceErrorCode != ErrorCode.None)
        {
            return new (updateAttendanceErrorCode, 0);
        }

        var (sendMailErrorCode, mailId) = await SendToMailDailyReward(playerId, todayAttendanceBook.DayCount);
        if (sendMailErrorCode != ErrorCode.None)
        {
            var rollbackErrorCode = await _gameDb.UpdateAttendanceBook(attendanceBook);
            if (rollbackErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(new { PlayerId = playerId }, "RollBackError " + rollbackErrorCode.ToString());
            }
            return new(sendMailErrorCode, 0);
        }

        return new(ErrorCode.None, mailId) ;
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
            ItemCode1 = reward.Code,
            ItemCount1 = reward.Count
        };

        return await _gameDb.SendMail(mail);
    }
}

