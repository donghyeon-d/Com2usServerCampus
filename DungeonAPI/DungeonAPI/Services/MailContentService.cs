﻿using System;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;

namespace DungeonAPI.Services;

public class MailContentService : GameDb, IMailContentService
{
    readonly ILogger<MailContentService> _logger;

    public MailContentService(ILogger<MailContentService> logger, IOptions<DbConfig> dbConfig)
        : base(logger, dbConfig)
	{
        _logger = logger;
	}

    public async Task<ErrorCode> CreateMailContent(Int32 mailId, String content)
    {
        // TODO : log
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
            return ErrorCode.MailContentCreateFailException;
        }
        finally
        {
            Dispose();
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
            // TODO: log
            return new Tuple<ErrorCode, MailContent>(ErrorCode.MailContentLoadFailException, null);
        }
        finally
        {
            Dispose();
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
            // TODO: log
            return ErrorCode.MailContentDeleteFailException;
        }
        finally
        {
            Dispose();
        }
    }
}

