using System;
using System.Security.Cryptography;
using System.Text;

namespace DungeonAPI.Util;

public class Security
{
    private const string _allowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

    public static string MakeHashingPassWord(string saltValue, string pw)
    {
        var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(saltValue + pw));
        var stringBuilder = new StringBuilder();
        foreach (var b in hash)
        {
            stringBuilder.AppendFormat("{0:x2}", b);
        }

        return stringBuilder.ToString();
    }

    public static string MakeSaltString()
    {
        var bytes = new byte[64];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(bytes);
        }

        return new string(bytes.Select(x => _allowableCharacters[x % _allowableCharacters.Length]).ToArray());
    }

    public static string CreateAuthToken()
    {
        var bytes = new byte[25];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(bytes);
        }

        return new string(bytes.Select(x => _allowableCharacters[x % _allowableCharacters.Length]).ToArray());
    }
}

