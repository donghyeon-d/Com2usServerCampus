using DungeonAPI.ModelDB;
using System.ComponentModel.DataAnnotations;

namespace DungeonAPI.RequestResponse;

public class PlayerItemListRes
{
    public ErrorCode Result { get; set; }
    public List<Item> ItemList { get; set; }
}

