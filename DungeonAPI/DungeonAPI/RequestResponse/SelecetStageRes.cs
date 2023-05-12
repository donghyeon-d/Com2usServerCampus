using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class SelecetStageRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public List<StageItem>? ItemList { get; set; } = null;
    public List<StageNPC>? NPCList { get; set; } = null;
}

public class StageItem
{
    public Int32 ItemCode { get; set; }
    public Int32 Count { get; set; }
}

public class StageNPC
{
    public Int32 NPCCode { get; set; }
    public Int32 Count { get; set; }
}