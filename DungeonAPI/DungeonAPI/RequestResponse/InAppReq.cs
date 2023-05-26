using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class InAppReq : AuthPlayerRequest
{
    [Required]
    public String ReceiptId { get; set; } = "";
}

