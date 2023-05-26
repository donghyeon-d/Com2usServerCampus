using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class StageListRes : AuthPlayerResponse
{
    public CompletedDungeon? CompleteStage { get; set; } = null;
}
