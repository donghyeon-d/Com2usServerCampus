// mail 유효기간, 리스트(페이지)당 메일 개수 정해야 
using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IMailDb
{
    // insert하기. content, reward 두 서비스 먼저 만들고 내부에서 그거 호출
    public Task<Tuple<ErrorCode, Int32>> CreateMail(Mail mail, MailContent content, List<MailReward> rewards);

    // 몇번째 리스트(페이지)의 몇번째 꺼 
    public Task<Tuple<ErrorCode, List<Mail>>> LoadMailListAtPage(Int32 playerId, Int32 pageIndex);

    // Delete
    public Task<ErrorCode> MarkAsDeleteMail(Int32 MailId);

    public Task<ErrorCode> OpenMail(Int32 MailId);

    // 수령 완료
    public Task<ErrorCode> MarkAsReceivedReward(Int32 MailId);
}

