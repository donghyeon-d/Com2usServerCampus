using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class KillNPCReq : AuthPlayerRequest
{
    [Required]
    public Int32 KilledNPCCode { get; set; }
}
