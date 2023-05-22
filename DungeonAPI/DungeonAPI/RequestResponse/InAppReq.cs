using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class InAppReq : AuthPlayerRequest
{
   public String ReceiptId { get; set; } = "";
}

