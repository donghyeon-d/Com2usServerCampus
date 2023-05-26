using System;
namespace DungeonAPI.ModelDB;

public class InAppPurchase
{
    public Int32 PlayerId { get; set; }
    public String ReceiptId { get; set; }
    public DateTime ReceiveDate { get; set; }
    public Int32 ProductCode { get; set; }
}

