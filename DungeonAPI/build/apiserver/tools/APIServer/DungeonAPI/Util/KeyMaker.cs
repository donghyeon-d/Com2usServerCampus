namespace DungeonAPI.Util;

public class KeyMaker
{
    public static string MakeKillNPCKey(Int32 playerId)
    {
        return "P" +playerId + "Kill";
    }

    public static string MakeFarmingItemKey(Int32 playerId)
    {
        return "P" +playerId + "Farming";
    }

    public static string MakePlayerInfoKey(Int32 playerId)
    {
        return "P" +playerId + "Info";
    }

    public static string MakeInDungeonKey(Int32 playerId)
    {
        return "P" +playerId + "Dungeon";
    }

    public static string MakeNoticeKey()
    {
        return "NoticeKey";
    }
}
