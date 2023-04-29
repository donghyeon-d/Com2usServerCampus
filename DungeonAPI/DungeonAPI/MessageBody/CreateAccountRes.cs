using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.MessageBody;

public class CreateAccountRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
}
