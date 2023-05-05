using System;
using DungeonAPI.MessageBody;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ReceiveAttendanceRewardController : ControllerBase
{
	readonly ILogger<ReceiveAttendanceRewardController> _logger;
	readonly IAttendanceBookDb _attendanceBookDb;

	public ReceiveAttendanceRewardController(ILogger<ReceiveAttendanceRewardController> logger,
        IAttendanceBookDb attendanceBookDb)
	{
		_logger = logger;
		_attendanceBookDb = attendanceBookDb;
    }

    [HttpPost]
    public async Task<ReceiveAttendanceRewardRes> LoadAttendanceBook(ReceiveAttendanceRewardReq request)
	{
		ReceiveAttendanceRewardRes response = new ReceiveAttendanceRewardRes();

        var playerIdValue = HttpContext.Items["PlayerId"];
        Int32 playerId = int.Parse(playerIdValue.ToString());

        response.Result = await _attendanceBookDb.ReceiveRewardToMail(playerId);
        return response;
    }
}

