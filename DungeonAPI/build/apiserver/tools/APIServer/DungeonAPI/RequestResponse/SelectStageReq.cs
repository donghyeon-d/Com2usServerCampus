using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class SelectStageReq : AuthPlayerRequest
{
    [Required]
    public Int32 StageCode { get; set; }
}
