namespace DungeonAPI.ModelDB;

public class PlayerInfo
{
    public String AuthToken { get; set; } = "";
    public Int32 Id { get; set; } = 0;
    public String Status { get; set; } = "";
    public Int32 CurrentStage { get; set; } = 0;
}
