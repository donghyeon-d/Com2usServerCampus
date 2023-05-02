using System;
using DungeonAPI.MessageBody;
using DungeonAPI.Services;
using DungeonAPI.MessageBody;
using Microsoft.AspNetCore.Mvc;

namespace DungeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CreateNoticeController
{
    readonly ILogger<CreateNoticeController> _logger;
    readonly INotice _notice;
    
    public CreateNoticeController(ILogger<CreateNoticeController> logger, INotice notice)
	{
        _logger = logger;
        _notice = notice;
	}

    [HttpPost]
    public async Task<CreateNoticeReq> CreateNotice(CreateNoticeRes request)
    {

    }
}

