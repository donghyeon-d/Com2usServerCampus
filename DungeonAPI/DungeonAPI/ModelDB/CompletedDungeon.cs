using DungeonAPI.Services;

namespace DungeonAPI.ModelDB;

public class CompletedDungeon
{
    public Int32 PlayerId { get; set; }
    public String Thema { get; set; } = "";
    public Int32 Stage { get; set; }
}
