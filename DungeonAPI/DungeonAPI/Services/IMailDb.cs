// mail 유효기간, 리스트(페이지)당 메일 개수 정해야 
using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IMailDb
{
    public Task<Tuple<ErrorCode, Int32>> CreateMail(Mail mail, MailContent content, List<MailReward> rewards);
    public Task<Tuple<ErrorCode, List<Mail>>> LoadMailListAtPage(Int32 playerId, Int32 pageIndex);
    public Task<ErrorCode> MarkAsDeleteMail(Int32 MailId, Int32 playerId);
    public Task<ErrorCode> MarkAsOpenMail(Int32 MailId, Int32 playerId);
    public Task<ErrorCode> MarkAsReceivedReward(Int32 MailId, Int32 playerId);
    public Task<ErrorCode> MarkAsNotReceivedReward(Int32 MailId, Int32 playerId);
    public Task<ErrorCode> CheckPlayerHasMail(Int32 playerId, Int32 mailId);
}

