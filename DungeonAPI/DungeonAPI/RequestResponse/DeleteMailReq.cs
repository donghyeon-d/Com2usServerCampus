using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class DeleteMailReq : AuthPlayerRequest
{
    [Required]
    public Int32 MailId { get; set; }
}

