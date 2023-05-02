using System;
namespace DungeonAPI.ModelDB;

public class Notification
{
	public DateTime Date { get; set; } = DateTime.Now;
	public string Title { get; set; } = "";
	public string Content { get; set; } = "";
}

