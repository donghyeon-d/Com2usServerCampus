using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class SelectStageRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public List<FarmingItem>? ItemList { get; set; } = null;
    public List<KillNPC>? NPCList { get; set; } = null;
}
