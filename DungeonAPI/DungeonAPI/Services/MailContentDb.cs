using System;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using ZLogger;

namespace DungeonAPI.Services;

public class MailContentDb : GameDb, IMailContentDb
{
    readonly ILogger<MailContentDb> _logger;

    public MailContentDb(ILogger<MailContentDb> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
	{
        _logger = logger;
	}

    public async Task<ErrorCode> CreateMailContent(Int32 mailId, String content)
    {
        try
        {
            var count = await _queryFactory.Query("MailContent").InsertAsync(new { mailId, content });
            if (count != 1)
            {
                return ErrorCode.MailContentCreateFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.MailContentCreateFailException;
        }
    }

    public async Task<Tuple<ErrorCode, MailContent>> LoadMailContent(Int32 mailId)
    {
        try
        {
            var mailContent = await _queryFactory.Query("MailContent")
                                           .Where("MailId", mailId)
                                           .FirstOrDefaultAsync<MailContent>();
            if (mailContent is null)
            {
                return new Tuple<ErrorCode, MailContent>(ErrorCode.MailContentLoadFail, null);
            }
            return new Tuple<ErrorCode, MailContent>(ErrorCode.None, mailContent);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new Tuple<ErrorCode, MailContent>(ErrorCode.MailContentLoadFailException, null);
        }
    }

    public async Task<ErrorCode> DeleteMailContent(Int32 mailId)
    {
        try
        {
            var mailContent = await _queryFactory.Query("MailContent")
                                           .Where("MailId", mailId)
                                           .DeleteAsync();
            if (mailContent != 1)
            {
                return ErrorCode.MailContentDeleteFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);// TODO: log
            return ErrorCode.MailContentDeleteFailException;
        }
    }
}

