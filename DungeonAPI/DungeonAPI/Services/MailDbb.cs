using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;

namespace DungeonAPI.Services;

public class MailDbb : GameDb, IMailDb
{
    readonly ILogger<MailDbb> _logger;
    readonly IMailContentDb _mailContent;
    int _expireDay = 30;
    int _listCount = 2;

    public MailDbb(ILogger<MailDbb> logger, IOptions<DbConfig> dbConfig,
        IMailContentDb mailContent)
        : base(logger, dbConfig)
    {
        _logger = logger;
        _mailContent = mailContent;
    }

    public async Task<Tuple<ErrorCode, Int32>> CreateMail(Mail mail, MailContent content)
    {
        Open();
        try
        {
            int mailId = await _queryFactory.Query("Mail")
                                        .InsertGetIdAsync<Int32>(mail);

            ErrorCode mailContentErrorCode = await _mailContent.CreateMailContent(mailId, content.Content);
            if (mailContentErrorCode != ErrorCode.None)
            {
                await DeleteMail(mailId);
                return new Tuple<ErrorCode, Int32>(mailContentErrorCode, mailId);
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
        Open();
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
            if (result is null)
            {
                return new Tuple<ErrorCode, List<Mail>>(ErrorCode.MailLoadFailNotFound, null);
            }

            List<Mail> mails = result.ToList();
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

    public async Task<ErrorCode> MarkAsDeleteMail(Int32 mailId)
    {
        Open();
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
        Open();
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
        Open();
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


}
