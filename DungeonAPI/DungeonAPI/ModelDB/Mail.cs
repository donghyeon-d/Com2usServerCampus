using System;
namespace DungeonAPI.ModelDB;

public class Mail
{
	public Int32 PlayerId { get; set; }
	public Int32 MailId { get; set; }
	public String Title { get; set; }
	public Int32 ContentID { get; set; }
	public DateTime PostDate { get; set; }
	public DateTime ExpiredDate { get; set; }
	public Byte IsOpened { get; set; }
    public Byte IsReceivedReward { get; set; }
	public Byte IsDeleted { get; set; }
    public Byte CanDelete { get; set; }
    public String Sender { get; set; }
	public Int32? RewardItemBaseCode1 { get; set; } = null;
	public Int32? Item1Count { get; set; } = null;
    public Int32? RewardItemBaseCode2 { get; set; } = null;
    public Int32? Item2Count { get; set; } = null;
    public Int32? RewardItemBaseCode3 { get; set; } = null;
    public Int32? Item3Count { get; set; } = null;
    public Int32? RewardItemBaseCode4 { get; set; } = null;
    public Int32? Item4Count { get; set; } = null;
}

