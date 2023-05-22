using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.RequestResponse;

public class EnhanceItemRes : AuthPlayerResponse
{
    public Item? ResultItem { get; set; } = null;
}

