namespace DungeonAPI.RequestResponse;

public class StageListRes : AuthPlayerResponse
{
    public List<Int32>? StageCodeList { get; set; } = null;
}
