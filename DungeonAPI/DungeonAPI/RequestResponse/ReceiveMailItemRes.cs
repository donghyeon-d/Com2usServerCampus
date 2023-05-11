using System;
using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class ReceiveMailItemRes
{
    [Required]
    public ErrorCode Result { get; set; } = ErrorCode.None;
}

