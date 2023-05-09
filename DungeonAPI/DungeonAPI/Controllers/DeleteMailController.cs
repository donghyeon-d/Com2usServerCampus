using System;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DeleteMailController : ControllerBase
{
    readonly ILogger<DeleteMailController> _logger;
    readonly IMailDb _mailDb;

    public DeleteMailController(ILogger<DeleteMailController> logger,
        IMailDb mailDb)
	{
        _logger = logger;
        _mailDb = mailDb;
	}

    [HttpPost]
    public async Task<DeleteMailRes> MarkAsDeleteMail(DeleteMailReq request)
    {
        DeleteMailRes response = new DeleteMailRes();

        Int32 playerId = int.Parse(HttpContext.Items["PlayerId"].ToString());

        var MarkAsDeleteMailErrorCode = await _mailDb.MarkAsDeleteMail(request.MailId, playerId);
        if (MarkAsDeleteMailErrorCode != ErrorCode.None)
        {
            response.Result = MarkAsDeleteMailErrorCode;
            return response;
        }

        return response;
    }
}

