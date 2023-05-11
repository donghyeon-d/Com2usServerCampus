using DungeonAPI.ModelDB;
namespace DungeonAPI.RequestResponse;

public class ReadMailListRes
{
    public List<MailInfo>? Mails { get; set; } = null;

    public ErrorCode Result { get; set; } = ErrorCode.None;
}
