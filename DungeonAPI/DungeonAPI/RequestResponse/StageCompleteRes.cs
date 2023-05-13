using DungeonAPI.ModelDB;
namespace DungeonAPI.RequestResponse;

public class StageCompleteRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;

    public List<FarmingItem>? RewardList = null;
}
