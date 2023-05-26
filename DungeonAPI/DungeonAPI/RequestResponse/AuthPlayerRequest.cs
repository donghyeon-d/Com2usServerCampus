using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class AuthPlayerRequest
{
    [Required]
    public Int32 PlayerId { get; set; } = 0;
    [Required]
    public String AuthToken { get; set; } = "";
    [Required]
    public String AppVersion { get; set; } = "";
    [Required]
    public String MasterDataVersion { get; set; } = "";
}
