using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class SelectStageRes : AuthPlayerResponse
{
    public List<FarmingItem>? ItemList { get; set; } = null;
    public List<KillNPC>? NPCList { get; set; } = null;
}
