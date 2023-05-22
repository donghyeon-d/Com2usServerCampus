using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class LoadAttendanceBookReq : AuthPlayerRequest
{
    [Required]
    public String MasterDataVersion { get; set; } = "";
}

