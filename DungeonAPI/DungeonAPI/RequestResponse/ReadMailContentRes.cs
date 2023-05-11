using System;
using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class ReadMailContentRes
{
    public MailContent? MailContent { get; set; } = null;
    public ErrorCode Result { get; set; } = ErrorCode.None;
}

