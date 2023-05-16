using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class StageListRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public List<Int32>? StageCodeList { get; set; } = null;
}
