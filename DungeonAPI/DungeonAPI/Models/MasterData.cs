using System.Net.NetworkInformation;

namespace DungeonAPI.Models;

public class MasterData
{
    public static IEnumerable<Item> _item { get; set; }
    public static ItemAttribute _itemAttribute { get; set; }
    public static AttendanceReward _attendanceReward { get; set; }
    public static InAppProduct _inAppProduct { get; set; }
    public static StageItem _stageItem { get; set; }
    public static StageAttackNPC _stageAttackNPC { get; set; }

    public class Item
    {
        public Int32 Code { get; set; }
        public string Name { get; set; }
        public Int32 Attribute { get; set; }
        public Int64 Sell { get; set; }
        public Int64 Buy { get; set; }
        public Int32 UseLv { get; set; }
        public Int64 Attack { get; set; }
        public Int64 Defence { get; set; }
        public Int64 Magic { get; set; }
        public Byte EnhanceMaxCount { get; set; }
        public Int32 MaxStack { get; set; }
    }

    public class ItemAttribute
    {
        public String Name { get; set; }
        public Int32 Code { get; set; }
    }

    public class AttendanceReward
    {
        public Int32 Code { get; set; }
        public Int32 ItemCode { get; set; }
        public Int32 Count { get; set; }
    }

    public class InAppProduct
    {
        public Int32 Code { get; set; }
        public Int32 ItemCode { get; set; }
        public String ItemName { get; set; }
        public Int32 ItemCount { get; set; }
    }

    public class StageItem
    {
        public Int32 Code { get; set; }
        public Int32 ItemCode { get; set; }
    }
     
    public class StageAttackNPC
    {
        public Int32 Code { get; set; }
        public Int32 NPCCode { get; set; }
        public Int32 NPCCount { get; set; }
        public Int32 Exp { get; set; }
    }
}