using System;
namespace DungeonAPI.ModelDB;

public class Mail
{
	public Int32 PlayerId { get; set; }
	public Int32 MailId { get; set; }
	public String Title { get; set; }
	public Int32 ContentId { get; set; }
	public Int32 RewardId { get; set; }
	public DateTime PostDate { get; set; }
	public DateTime ExpiredDate { get; set; }
	public Byte IsReceived { get; set; }
	public Byte IsDeleted { get; set; }
    public Byte CanDelete { get; set; }
    public String Sender { get; set; }
}

