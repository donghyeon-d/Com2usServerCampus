using System;
namespace DungeonAPI.ModelDB;

public class Item
{
	public Int32 PlayerId { get; set; }
	public Int32 ItemId { get; set; }
	public Int32 ItemMasterCode { get; set; }
	public Int32 ItemCount { get; set; }
	public Int64 Attack { get; set; }
	public Int64 Defence { get; set; }
	public Int64 Magic { get; set; }
	public Byte EnhanceLevel { get; set; }
	public Byte RemainingEnhanceCount { get; set; }
	public Byte Destructed { get; set; }
}

