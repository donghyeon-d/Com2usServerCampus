using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class StageListRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public List<CompletedDungeon>? CompleteList { get; set; } = null;
}
