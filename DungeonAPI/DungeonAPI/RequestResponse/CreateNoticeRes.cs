using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class CreateNoticeRes
{
    [Required]
    public ErrorCode ErrorCode { get; set; } = ErrorCode.None;
}

