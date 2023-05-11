using DungeonAPI.Services;
namespace DungeonAPI.ModelDB;

public class Item
{
	public Int32 PlayerId { get; set; }
	public Int32 ItemId { get; set; }
	public Int32 ItemCode { get; set; }
	public Int32 ItemCount { get; set; }
	public Int64 Attack { get; set; }
	public Int64 Defence { get; set; }
	public Int64 Magic { get; set; }
	public Byte EnhanceLevel { get; set; } = 0;
	public Byte EnhanceTryCount { get; set; } = 0;
	public Boolean IsDestructed { get; set; } = false;
	public Boolean IsDeleted { get; set; } = false;
    
    static public Item? InitItem(Int32 playerId, Int32 itemCode, Int32 itemCount)
    {
        MasterData.BaseItem? baseItem = MasterDataDb.s_baseItem.Find(tem => tem.Code == itemCode);
        if (baseItem is null)
        {
            return null;
        }

        Item item = new()
        {
            PlayerId = playerId,
            ItemCode = itemCode,
            ItemCount = itemCount,
            Attack = baseItem.Attack,
            Defence = baseItem.Defence,
            Magic = baseItem.Magic
        };

        return item;
    }
}
