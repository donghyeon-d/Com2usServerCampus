using System;
using System.ComponentModel.DataAnnotations;
using DungeonAPI.ModelDB;
namespace DungeonAPI.RequestResponse;

public class LoadMailListRes
{
    [Required]
    public List<Mail>? Mails { get; set; } = null;

    [Required]
    public ErrorCode Result { get; set; } = ErrorCode.None;
}

//public class MailWithReward
//{
//    public Int32 PlayerId { get; set; }
//    public Int32 MailId { get; set; }
//    public String Title { get; set; }
//    public Int32 ContentId { get; set; }
//    public DateTime PostDate { get; set; }
//    public DateTime ExpiredDate { get; set; }
//    public Byte IsReceived { get; set; }
//    public Byte IsDeleted { get; set; }
//    public Byte CanDelete { get; set; }
//    public String Sender { get; set; }
    //public List<MailReward> Rewards { get; set; }

    //public MailWithReward(Mail mail)
    //{
    //    PlayerId = mail.PlayerId;
    //    MailId = mail.MailId;
    //    Title = mail.Title;
    //    ContentId = mail.ContentId;
    //    PostDate = mail.PostDate;
    //    ExpiredDate = mail.ExpiredDate;
    //    IsReceived = mail.IsReceived;
    //    IsDeleted = mail.IsDeleted;
    //    CanDelete = mail.CanDelete;
    //    Sender = mail.Sender;
    //}
//}
