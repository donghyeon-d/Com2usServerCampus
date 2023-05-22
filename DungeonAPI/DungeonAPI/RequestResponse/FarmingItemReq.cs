using System.ComponentModel.DataAnnotations;
using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class FarmingItemReq : AuthPlayerRequest
{
    [Required]
    public Int32 ItemCode { get; set; }
}
