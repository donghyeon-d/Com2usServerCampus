using Humanizer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace DungeonAPI.ModelDB;

public class MasterData
{
    public class Meta
    {
        public Int32 Version { get; set; }
    }

    public class BaseItem
    {
        public Int32 Code { get; set; }
        public String Name { get; set; } = "";
        public Int32 Attribute { get; set; }
        public Int32 Sell { get; set; }
        public Int32 Buy { get; set; }
        public Int32 UseLv { get; set; }
        public Int32 Attack { get; set; }
        public Int32 Defence { get; set; }
        public Int32 Magic { get; set; }
        public Byte EnhanceMaxCount { get; set; }
        public Int32 MaxStack { get; set; }
    }

    public class ItemAttribute
    {
        public String Name { get; set; } = "";
        public Int32 Code { get; set; }
    }

    public class AttendanceReward
    {
        public Int32 Code { get; set; }
        public Int32 Day { get; set; }
        public Int32 ItemCode { get; set; }
        public Int32 Count { get; set; }
    }

    public class InAppProduct
    {
        public Int32 Code { get; set; }
        public Int32 ItemCode { get; set; }
        public String ItemName { get; set; } = "";
        public Int32 ItemCount { get; set; }
    }

    public class DungeonStage
    {
        public Int32 StageCode { get; set; }
        public String Thema { get; set; } = "";
        public Int32 Stage { get; set; }
    }

    public class StageItem
    {
        public Int32 StageCode { get; set; }
        public Int32 ItemCode { get; set; }
        public Int32 Count { get;set; }
    }
     
    public class StageAttackNPC
    {
        public Int32 StageCode { get; set; }
        public Int32 NPCCode { get; set; }
        public Int32 Count { get; set; }
        public Int32 Exp { get; set; }
    }

}
