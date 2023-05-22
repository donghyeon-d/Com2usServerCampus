using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class ReadMailContentReq : AuthPlayerRequest
{
    [Required]
    public Int32 MailId { get; set; }
}

