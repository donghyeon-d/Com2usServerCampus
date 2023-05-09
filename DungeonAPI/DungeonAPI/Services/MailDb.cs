using System;
using DungeonAPI.Configs;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using System.Data;
using DungeonAPI.ModelDB;
using Microsoft.AspNetCore.Http;

namespace DungeonAPI.Services;

public class MailDb : GameDb, IMailDb
{
    // mail 유효기간, 리스트(페이지)당 메일 개수 정해야 
    readonly ILogger<MailDb> _logger;
    readonly IMailContentDb _mailContent;
    readonly IMailRewardDb _mailReward;
    int _expireDay = 30;
    int _listCount = 2;

    public MailDb(ILogger<MailDb> logger, IOptions<DbConfig> dbConfig,
        IMailContentDb mailContent, IMailRewardDb mailReward)
        : base(logger, dbConfig)
    {
        _logger = logger;
        _mailContent = mailContent;
        _mailReward = mailReward;
    }

    // insert하기. content, reward 두 서비스 먼저 만들고 내부에서 그거 호출
    public async Task<Tuple<ErrorCode, Int32>> CreateMail(Mail mail, MailContent content, List<MailReward> rewards)
    {
        try
        {
            int mailId = await _queryFactory.Query("Mail").InsertGetIdAsync<Int32>(new
                                                        {
                                                            PlayerId = mail.PlayerId,
                                                            Title = mail.Title,
                                                            PostDate = mail.PostDate,
                                                            ExpiredDate = mail.ExpiredDate,
                                                            IsOpened = mail.IsOpened,
                                                            IsReceivedReward = mail.IsReceivedReward,
                                                            IsDeleted = mail.IsDeleted,
                                                            CanDelete = mail.CanDelete,
                                                            Sender = mail.Sender
                                                        }) ;
            _queryFactory.Query("Mail").

            ErrorCode mailContentErrorCode = await _mailContent.CreateMailContent(mailId, content.Content);
            if (mailContentErrorCode != ErrorCode.None)
            {
                await Rollback(mailId);
                return new Tuple<ErrorCode, Int32>(mailContentErrorCode, mailId);
            }

            ErrorCode mailRewardErrorCode = await _mailReward.CreateMailRewards(mailId, rewards);
            if (mailRewardErrorCode != ErrorCode.None)
            {
                await Rollback(mailId);
                return new Tuple<ErrorCode, Int32>(mailRewardErrorCode, mailId);
            }
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, mailId);
        }
        catch (Exception e)
        {
            return new Tuple<ErrorCode, Int32>(ErrorCode.MailCreateFailException, -1);
        }
        finally
        {
            Dispose();
        }
    }

    // 몇번째 리스트(페이지)의 몇번째 꺼 
    public async Task<Tuple<ErrorCode, List<Mail>>> LoadMailListAtPage(Int32 playerId, Int32 pageNumber)
    {
        try
        {
                pageNumber--; //페이지가 1페이지부터 시작이니까
            Int32 start = (pageNumber * _listCount);
            var result = await _queryFactory.Query("Mail")
                                           .Where("PlayerId", playerId)
                                           .WhereDate("ExpiredDate", ">", DateTime.Now)
                                           .Where("IsReceivedReward", 0)
                                           .Where("IsDeleted", 0)
                                           .OrderByDesc("PostDate")
                                           .Limit(_listCount)
                                           .Offset(start)
                                           .GetAsync<Mail>();

            List<Mail> mails = result.ToList(); // 비어있을리는 없나?
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
    public async Task<ErrorCode> MarkAsDeleteMail(Int32 mailId)
    {
        try
        {
            int count = await _queryFactory.Query("Mail")
                                            .Where("MailId", mailId)
                                            .Where("CanDelete", 1)
                                            .UpdateAsync(new { IsDeleted = 1 });
            if (count != 1)
            {
                return ErrorCode.MailDeleteFailNotExistOrCannotDelete;
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

    // 열기 (content 받아 가)
    public async Task<ErrorCode> MarkAsOpenMail(Int32 MailId)
    {
        try
        {
            int count = await _queryFactory.Query("Mail")
                                            .Where("MailId", MailId)
                                            .WhereDate("ExpiredDate", ">", DateTime.Now)
                                            .Where("IsDeleted", 0)
                                            .UpdateAsync(new { IsOpened = 1 });
            if (count != 1)
            {
                return ErrorCode.MailReceivedFailNotExistOrAlreadyOpen;
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

    // 수령 완료
    public async Task<ErrorCode> MarkAsReceivedReward(Int32 MailId)
    {
        try
        {
            int count = await _queryFactory.Query("Mail")
                                            .Where("MailId", MailId)
                                            .WhereDate("ExpiredDate", ">", DateTime.Now)
                                            .Where("IsReceivedReward", 0)
                                            .Where("IsDeleted", 0)
                                            .UpdateAsync(new { IsReceivedReward = 1 });
            if (count != 1)
            {
                return ErrorCode.MailReceivedFailNotExistOrAlreadyRecieve;
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

    async Task<ErrorCode> DeleteMail(Int32 mailId)
    {
        Open();
        try
        {
            int count = await _queryFactory.Query("Mail")
                                            .Where("MailId", mailId)
                                            .DeleteAsync();
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

    async Task Rollback(Int32 mailId)
    {
        await DeleteMail(mailId);
        await _mailContent.DeleteMailContent(mailId);
        await _mailReward.DeleteMailReward(mailId);
    }
}

