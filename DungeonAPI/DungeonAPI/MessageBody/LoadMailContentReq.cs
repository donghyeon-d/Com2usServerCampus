using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.MessageBody;

public class LoadMailContentReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "EMAIL IS NOT VALID")]
    public String Email { get; set; }

    [Required]
    public String AuthToken { get; set; }

    [Required]
    public String AppVersion { get; set; }

    [Required]
    public String MasterDataVersion { get; set; }

    [Required]
    public Int32 MailId { get; set; }
}

