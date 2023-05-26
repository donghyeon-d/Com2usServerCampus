using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class ReceiveMailItemReq : AuthPlayerRequest
{
    [Required]
    public Int32 MailId { get; set; }
}

