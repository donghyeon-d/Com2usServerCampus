using System;
using System.ComponentModel.DataAnnotations;
using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class LoginRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public Int32 playerId { get; set; }
    [Required] public String AuthToken { get; set; } = "";
    [Required] public List<Item>? Item { get; set; } = null;
    [Required] public Player? Player { get; set; } = null;
}

