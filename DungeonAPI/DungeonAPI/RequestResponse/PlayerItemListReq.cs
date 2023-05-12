using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class PlayerItemListReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "EMAIL IS NOT VALID")]
    public string Email { get; set; } = "";

    [Required]
    public string AuthToken { get; set; } = "";

    [Required]
    public string AppVersion { get; set; } = "";

    [Required]
    public string MasterDataVersion { get; set; } = "";
}
