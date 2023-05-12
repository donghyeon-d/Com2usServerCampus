using System;
using CloudStructures;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using CloudStructures.Structures;

namespace DungeonAPI.Services;

public class NoticeMemeoryDb : INoticeMemoryDb
{
    static string s_notificationKey { get; set; } = "noticekey";
    readonly RedisConnection _redisConn;
    readonly ILogger<NoticeMemeoryDb> _logger;
    readonly IOptions<DbConfig> _dbConfig;
    readonly TimeSpan defaultExpiry = TimeSpan.FromDays(1); // TODO: Expiry setting

    public NoticeMemeoryDb(ILogger<NoticeMemeoryDb> logger, IOptions<DbConfig> dbConfig)
	{
        _logger = logger;
        _dbConfig = dbConfig;
        var redisAddress = _dbConfig.Value.Redis;
        var redisConfig = new RedisConfig("default", redisAddress);
        _redisConn = new RedisConnection(redisConfig);

        // TODO: log
    }

    public async Task<ErrorCode> CreateNotification(string title, string content, DateTime? dateTime = null)
    {
        // TODO: log
        try
        {
            var redis = new RedisList<Notification>(_redisConn, s_notificationKey, defaultExpiry);
            try
            {
                // Create Noti instance
                Notification noti = new ()
                {
                    Title = title,
                    Content = content,
                    Date = dateTime ?? DateTime.Today
                };

                await redis.LeftPushAsync(noti);
                return ErrorCode.None;
            }
            catch (Exception e)
            {
                // TODO: log
                _logger.LogError(e.Message);
                return ErrorCode.RedisFailException;
            }
        }
        catch (Exception e)
        {
            // TODO: log
            _logger.LogError(e.Message);
            return ErrorCode.RedisFailException;
        }
    }

    public async Task<Tuple<ErrorCode, List<Notification>?>> ReadNotificationList()
    {
        try
        {
            var redis = new RedisList<Notification>(_redisConn, s_notificationKey, defaultExpiry);
            try
            {
                var result = await redis.RangeAsync();
                List<Notification> notis = result.ToList();
                return new (ErrorCode.None, notis);
            }
            catch (Exception e)
            {
                // TODO: log
                _logger.LogError(e.Message);
                return new (ErrorCode.RedisFailException, null);
            }
        }
        catch (Exception e)
        {
            // TODO: log
            _logger.LogError(e.Message);
            return new (ErrorCode.RedisFailException, null);
        }
    }
}

