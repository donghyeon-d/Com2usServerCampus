using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class DeleteMailRes
{
    [Required]
    public ErrorCode Result { get; set; } = ErrorCode.None;
}

