// mail 유효기간, 리스트(페이지)당 메일 개수 정해야 
using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IMailService
{
    // insert하기. content, reward 두 서비스 먼저 만들고 내부에서 그거 호출
    public Task<ErrorCode> CreateMail(Mail mail, MailContent content, List<MailReward> rewards);

    // 몇번째 리스트(페이지)의 몇번째 꺼 
    public Task<Tuple<ErrorCode, List<Mail>>> LoadMailAt(Int32 userId, Int32 listIndex);

    // Delete
    public Task<ErrorCode> DeleteMail(Int32 MailId);

    // 수령 완료
    public Task<ErrorCode> ReceivedMail(Int32 MailId);
}

