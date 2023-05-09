using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.MessageBody;

public class CreateNoticeRes
{
    [Required]
    public ErrorCode ErrorCode { get; set; } = ErrorCode.None;
}

