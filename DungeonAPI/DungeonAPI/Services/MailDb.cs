using DungeonAPI.Configs;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using DungeonAPI.ModelDB;
using ZLogger;

namespace DungeonAPI.Services;

public class MailDb : GameDb, IMailDb
{
    // mail 유효기간, 리스트(페이지)당 메일 개수 정해야 
    readonly ILogger<MailDb> _logger;
    int _listCount = 2;

    public MailDb(ILogger<MailDb> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
    {
        _logger = logger;
    }

    public async Task<Tuple<ErrorCode, Int32>> SendMail(Mail mail)
    {
        try
        {
            int mailId = await _queryFactory.Query("Mail").InsertGetIdAsync<Int32>(mail);
            if (mailId == 0)
            {
                return new (ErrorCode.MailCreateFail, -1);
            }
            return new(ErrorCode.None, mailId);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new (ErrorCode.MailCreateFailException, -1);
        }
    }

    public async Task<Tuple<ErrorCode, Mail?>> LoadMail(Int32 mailId)
    {
        try
        {
            var result = await _queryFactory.Query("Mail")
                       .Where("MailId", mailId)
                       .FirstOrDefaultAsync<Mail>();

            if (result is null)
            {
                return new (ErrorCode.LoadMailFailNotExist, null);
            }

            return new (ErrorCode.None, result);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new (ErrorCode.LoadMailFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, List<MailInfo>?>> ReadMailListAtPage(Int32 playerId, Int32 pageNumber)
    {
        try
        {
            pageNumber--; // client는 1페이지부터 시작인데, db에는 0부터 저장 
            Int32 start = (pageNumber * _listCount);
            var result = await _queryFactory.Query("Mail")
                        .Select("MailId", "Title", "PostDate", "ExpiredDate", "IsOpend", 
                        "IsReceivedItem", "CanDelete", "Sender", "ItemCode", "ItemCount")
                        .Where("PlayerId", playerId)
                        .WhereDate("ExpiredDate", ">", DateTime.Now)
                        .Where("IsDeleted", false)
                        .OrderByDesc("MailId")
                        .Limit(_listCount)
                        .Offset(start)
                        .GetAsync<MailInfo>();

            List<MailInfo>? mails = result.ToList();
            return new (ErrorCode.None, mails);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new (ErrorCode.MailLoadFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, MailContent?>> ReadMailContent(Int32 playerId, Int32 mailId)
    {
        try
        {
            var result = await _queryFactory.Query("Mail")
                        .Select("PlayerId", "MailId", "Content")
                        .Where("MailId", mailId)
                        .OrderByDesc("MailId")
                        .FirstOrDefaultAsync<Mail>();

            if (result is null)
            {
                return new (ErrorCode.MailContentLoadFail, null);
            }

            if (result.PlayerId != playerId)
            {
                return new(ErrorCode.ReadMailContentWrongPlayer, null);
            }

            MailContent mailContent = new()
            {
                MailId = mailId,
                Content = result.Content
            };

            return new(ErrorCode.None, mailContent);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new(ErrorCode.MailContentLoadFailException, null);
        }
    }

    public async Task<ErrorCode> MarkAsDeleteMail(Int32 mailId, Int32 playerId)
    {
        try
        {
            int count = await _queryFactory.Query("Mail")
                                            .Where("MailId", mailId)
                                            .Where("PlayerId", playerId)
                                            .Where("CanDelete", true)
                                            .UpdateAsync(new { IsDeleted = true });
            if (count != 1)
            {
                return ErrorCode.MailDeleteFailNotExistOrCannotDelete;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.MailDeleteFailException;
        }
    }

    public async Task<ErrorCode> MarkAsOpenMail(Int32 MailId, Int32 playerId)
    {
        try
        {
            int count = await _queryFactory.Query("Mail")
                                            .Where("MailId", MailId)
                                            .Where("PlayerId", playerId)
                                            .Where("IsDeleted", false)
                                            .UpdateAsync(new { IsOpened = true });
            if (count != 1)
            {
                return ErrorCode.MailMarkAsOpenFailNotExist;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.MailMarkAsOpenFailException;
        }
    }

    public async Task<ErrorCode> MarkAsReceivedItem(Int32 MailId, Int32 playerId)
    {
        try
        {
            int count = await _queryFactory.Query("Mail")
                                            .Where("MailId", MailId)
                                            .Where("PlayerId", playerId)
                                            .UpdateAsync(new { 
                                                IsOpened = true,
                                                IsReceivedItem = true
                                            });
            if (count != 1)
            {
                return ErrorCode.MailMarkAsReceivedFailNotExist;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.MailMarkAsReceivedFailException;
        }
    }

    public async Task<ErrorCode> MarkAsNotReceivedItem(Int32 MailId, Int32 playerId)
    {
        try
        {
            int count = await _queryFactory.Query("Mail")
                                            .Where("MailId", MailId)
                                            .Where("PlayerId", playerId)
                                            .UpdateAsync(new { IsReceivedReward = false });
            if (count != 1)
            {
                return ErrorCode.MarkAsNotReceivedItemFailNotExist;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.MarkAsNotReceivedItemException;
        }
    }

    public async Task<ErrorCode> DeleteMail(Int32 mailId)
    {
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
            _logger.ZLogWarning(e.Message);
            return ErrorCode.MailDeleteFailException;
        }
    }
}

