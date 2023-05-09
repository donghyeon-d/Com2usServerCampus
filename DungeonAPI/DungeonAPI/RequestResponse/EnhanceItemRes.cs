using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.RequestResponse;

public class EnhanceItemRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;

    public Item ResultItem { get; set; }
}

