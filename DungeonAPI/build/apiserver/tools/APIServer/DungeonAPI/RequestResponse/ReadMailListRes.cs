using DungeonAPI.ModelDB;
namespace DungeonAPI.RequestResponse;

public class ReadMailListRes : AuthPlayerResponse
{
    public List<MailInfo>? Mails { get; set; } = null;
}
