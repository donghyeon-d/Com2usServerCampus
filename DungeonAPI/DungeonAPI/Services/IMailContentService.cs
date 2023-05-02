using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface IMailContentService
{
    public Task<ErrorCode> CreateMailContent(Int32 mailId, String content);

    public Task<Tuple<ErrorCode, MailContent>> LoadMailContent(Int32 mailId);

    public Task<ErrorCode> DeleteMailContent(Int32 mailId);
}

