using System;
namespace DungeonAPI.Services;

public class DbConfig
{
    public String AccountDb { get; set; }
    public String GameDb { get; set; }
    public String MasterDataDb { get; set; }
    public String Memcached { get; set; }
}