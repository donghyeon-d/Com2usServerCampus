using System;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.MessageBody;

public class CreateNoticeReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "EMAIL IS NOT VALID")]
    public String Email { get; set; }

    [Required]
    public String Password { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    public String Title { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    public String Content { get; set; }

    public String Date { get; set; } = DateTime.Now.ToString();

    [Required]
    public String AppVersion { get; set; }

    [Required]
    public String MasterDataVersion { get; set; }
}

