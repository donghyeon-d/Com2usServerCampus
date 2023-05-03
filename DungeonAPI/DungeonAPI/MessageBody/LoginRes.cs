using System;
using System.ComponentModel.DataAnnotations;
using DungeonAPI.ModelDB;

namespace DungeonAPI.MessageBody;

public class LoginRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public String AuthToken { get; set; } = "";
    [Required] public List<Item>? Item { get; set; } = null;
    [Required] public Player? Player { get; set; } = null;

    public void ResetThenSetErrorCode(ErrorCode errorCode)
    {
        Result = errorCode;
        AuthToken = "";
        Item = null;
        Player = null;
    }
}

