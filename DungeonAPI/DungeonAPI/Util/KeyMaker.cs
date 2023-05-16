namespace DungeonAPI.Util;

public class KeyMaker
{
    public static string MakeKillNPCKey(string email)
    {
        return email + "Kill";
    }

    public static string MakeFarmingItemKey(string email)
    {
        return email + "Farming";
    }

    public static string MakePlayerInfo(string email)
    {
        return email;
    }
}
