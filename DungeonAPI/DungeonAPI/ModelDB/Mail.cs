using System;
namespace DungeonAPI.ModelDB;

public class Mail
{
	public Int32 PlayerId { get; set; }
	public Int32 MailId { get; set; }
	public String Title { get; set; } = "";
    public String Content { get; set; } = "";
	public DateTime PostDate { get; set; } = DateTime.Now;
	public DateTime ExpiredDate { get; set; } = DateTime.Now.AddDays(60);
    public Boolean IsOpened { get; set; } = false;
    public Boolean IsReceivedItem { get; set; } = false;
	public Boolean IsDeleted { get; set; } = false;
    public Boolean CanDelete { get; set; } = true;
    public String Sender { get; set; } = "";
	public Int32 ItemCode { get; set; }
	public Int32 ItemCount { get; set; }

}

public class MailInfo
{
    public Int32 MailId { get; set; }
    public String Title { get; set; } = "";
    public DateTime PostDate { get; set; }
    public DateTime ExpiredDate { get; set; }
    public Boolean IsOpened { get; set; }
    public Boolean IsReceivedItem { get; set; }
    public Boolean CanDelete { get; set; }
    public String Sender { get; set; } = "";
    public Int32 ItemCode { get; set; }
    public Int32 ItemCount { get; set; }
}

public class MailContent
{
    public Int32 MailId { get; set; }
    public String Content { get; set; } = "";
}