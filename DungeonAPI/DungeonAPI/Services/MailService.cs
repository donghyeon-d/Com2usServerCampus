using System;
using DungeonAPI.Configs;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using System.Data;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public class MailService : GameDb, IMailService
{
    // mail 유효기간, 리스트(페이지)당 메일 개수 정해야 
    readonly ILogger<MailService> _logger;
    readonly IMailContentService _mailContent;
    readonly IMailRewardService _mailReward;
    int _expireDay = 30;
    int _listCount = 20;

    public MailService(ILogger<MailService> logger, IOptions<DbConfig> dbConfig,
        IMailContentService mailContent, IMailRewardService mailReward)
        : base(logger, dbConfig)
    {
        _logger = logger;
        _mailContent = mailContent;
        _mailReward = mailReward;
    }

    // insert하기. content, reward 두 서비스 먼저 만들고 내부에서 그거 호출
    public async Task<ErrorCode> CreateMail(Mail mail, MailContent content, List<MailReward> rewards)
    {
        try
        {
            int mailId = await _queryFactory.Query("Mail").InsertGetIdAsync<Int32>(new
                                                        {
                                                            UserId = mail.UserId,
                                                            Title = mail.Title,
                                                            PostDate = mail.PostDate,
                                                            ExpiredDate = mail.ExpiredDate,
                                                            IsReceived = mail.IsReceived,
                                                            IsDelted = mail.IsDeleted,
                                                            CanDelete = mail.CanDelete,
                                                            Sender = mail.Sender
                                                        }) ;

            ErrorCode mailContentErrorCode = await _mailContent.CreateMailContent(mailId, content.Content);
            if (mailContentErrorCode != ErrorCode.None)
            {
                await Rollback(mailId);
                return mailContentErrorCode;
            }

            ErrorCode mailRewardErrorCode = await _mailReward.CreateMailRewards(mailId, rewards);
            if (mailRewardErrorCode != ErrorCode.None)
            {
                await Rollback(mailId);
                return mailRewardErrorCode;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.MailCreateFailException;
        }
        finally
        {
            Dispose();
        }
    }

    // 몇번째 리스트(페이지)의 몇번째 꺼 
    public async Task<Tuple<ErrorCode, List<Mail>>> LoadMailAt(Int32 userId, Int32 listIndex)
    {
        // listIndex--; 페이지가 1페이지부터 시작이니까
        try
        {
            var result = await _queryFactory.Query("Mail")
                                           .Where("UserId", userId)
                                           .WhereDate("ExpiredDate", "<", DateTime.Now)
                                           .Where("IsReceived", 0)
                                           .Where("IsDeleted", 0)
                                           .GetAsync<Mail>();

            List<Mail> mails = result.OrderBy(x => x).Skip(_listCount * listIndex).Take(_listCount).ToList(); // 비어있을리는 없나?
            return new Tuple<ErrorCode, List<Mail>>(ErrorCode.None, mails);
        }
        catch (Exception e)
        {
            // TODO: log
            return new Tuple<ErrorCode, List<Mail>>(ErrorCode.MailLoadFailException, null);
        }
        finally
        {
            Dispose();
        }
    }

    // Delete
    public async Task<ErrorCode> DeleteMail(Int32 MailId)
    {
        try
        {
            int count = await _queryFactory.Query("Mail").Where("MailId", MailId).UpdateAsync(new { IsDeleted = 1 });
            if (count != 1)
            {
                return ErrorCode.MailDeleteFailNotExist;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            // TODO : log
            return ErrorCode.MailDeleteFailException;
        }
        finally
        {
            Dispose();
        }
    }

    // 수령 완료
    public async Task<ErrorCode> ReceivedMail(Int32 MailId)
    {
        try
        {
            int count = await _queryFactory.Query("Mail").Where("MailId", MailId).UpdateAsync(new { IsReceived = 1 });
            if (count != 1)
            {
                return ErrorCode.MailReceivedFailNotExist;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            // TODO : log
            return ErrorCode.MailReceivedFailException;
        }
        finally
        {
            Dispose();
        }
    }

    async Task Rollback(Int32 mailId)
    {
        await DeleteMail(mailId);
        await _mailContent.DeleteMailContent(mailId);
        await _mailReward.DeleteMailReward(mailId);
    }
}

