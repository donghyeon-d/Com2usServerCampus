using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class StageCompleteReq : AuthPlayerRequest
{
    [Required]
    public Int32 Stage { get; set;}
}
