﻿using System;
using DungeonAPI.MessageBody;
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

        var MarkAsDeleteMailErrorCode = await _mailDb.MarkAsDeleteMail(request.MailId);
        if (MarkAsDeleteMailErrorCode != ErrorCode.None)
        {
            response.Result = MarkAsDeleteMailErrorCode;
            return response;
        }

        return response;
    }
}
