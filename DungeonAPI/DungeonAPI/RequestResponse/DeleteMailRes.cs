using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.MessageBody;

public class DeleteMailRes
{
    [Required]
    public ErrorCode Result { get; set; } = ErrorCode.None;
}

