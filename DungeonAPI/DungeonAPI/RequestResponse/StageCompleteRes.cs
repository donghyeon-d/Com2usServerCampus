using DungeonAPI.ModelDB;
namespace DungeonAPI.RequestResponse;

public class StageCompleteRes : AuthPlayerResponse
{
    public List<FarmingItem>? RewardList = null;
}
