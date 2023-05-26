using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class ReadMailListReq : AuthPlayerRequest
{
    [Required]
    public Int32 ListNumber { get; set; }
}

