using System;
using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class ReadMailContentRes : AuthPlayerResponse
{
    public MailContent? MailContent { get; set; } = null;
}

