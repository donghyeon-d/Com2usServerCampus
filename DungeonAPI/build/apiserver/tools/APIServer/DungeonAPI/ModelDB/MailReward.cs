using System;
namespace DungeonAPI.ModelDB;

public class MailReward
{
	public Int32 MailId { get; set; }
	public Int32 MailRewardId { get; set; }	
	public Int32 BaseItemCode { get; set; }
	public Int32 ItemCount { get; set; }
	public bool IsReceived { get; set; }
}

