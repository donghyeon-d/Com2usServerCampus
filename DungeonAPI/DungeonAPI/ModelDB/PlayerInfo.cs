namespace DungeonAPI.ModelDB;

public class PlayerInfo
{
    public String AuthToken { get; set; } = "";
    public Int32 PlayerId { get; set; } = 0;
    public String Status { get; set; } = "";
    public Int32 StageCode { get; set; } = 0;
}
