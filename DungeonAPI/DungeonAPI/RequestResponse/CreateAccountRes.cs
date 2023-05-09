using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class CreateAccountRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
}
