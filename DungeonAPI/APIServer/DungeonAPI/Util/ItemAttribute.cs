using DungeonAPI.ModelDB;
using DungeonAPI.Services;

namespace DungeonAPI.Util
{
    public class ItemAttribute
    {
        static public bool IsEquipment(Int32 itemCode)
        {
            var itemKind = MasterDataDb.s_baseItem.Find(i => i.Code == itemCode);
            if (itemKind == null)
            {
                return false;
            }

            if (itemKind.Attribute == (int)Enum.ItemAttribute.Weapon
                || itemKind.Attribute == (int)Enum.ItemAttribute.Armor
                || itemKind.Attribute == (int)Enum.ItemAttribute.Clothing)
            {
                return true;
            }
            return false;
        }

        static public bool IsGold(Int32 itemCode)
        {
            var itemKind = MasterDataDb.s_baseItem.Find(i => i.Code == itemCode);
            if (itemKind.Attribute == 5)
            {
                return true;
            }
            return false;
        }

        static public bool IsComsumableItem(Int32 itemCode)
        {
            var itemKind = MasterDataDb.s_baseItem.Find(i => i.Code == itemCode);
            if (itemKind.Attribute == 4)
            {
                return true;
            }
            return false;
        }
    }
}
