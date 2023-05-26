using DungeonAPI.ModelDB;

namespace DungeonAPI.RequestResponse;

public class PlayerItemListRes : AuthPlayerResponse
{
    public List<Item>? ItemList { get; set; } = null;
}
