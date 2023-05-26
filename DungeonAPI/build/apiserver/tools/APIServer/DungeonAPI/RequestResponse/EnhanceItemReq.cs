using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class EnhanceItemReq : AuthPlayerRequest
{
    [Required]
    public Int32 ItemId { get; set; }
}

