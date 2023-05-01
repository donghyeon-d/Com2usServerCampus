using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.MessageBody;

public class LoginRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public String AuthToken { get; set; } = "";
}

