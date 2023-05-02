using System;
using CloudStructures;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CloudStructures.Structures;

namespace DungeonAPI.Services;

public class Notice : INotice
{
    static string s_notificationKey { get; set; } = "key";
    DateTime today = DateTime.Now;
    readonly RedisConnection _redisConn;
    readonly ILogger<Notice> _logger;
    readonly IOptions<DbConfig> _dbConfig;
    TimeSpan defaultExpiry = TimeSpan.FromDays(1); // TODO: Expiry setting

    public Notice(ILogger<Notice> logger, IOptions<DbConfig> dbConfig)
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
                Notification noti = new Notification();
                noti.Title = title;
                noti.Content = content;
                noti.Date = dateTime ?? DateTime.Today;

                // push to redis
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

    // pop all data
    public async Task<Tuple<ErrorCode, List<Notification>>> LoadAllNotification()
    {
        try
        {
            var redis = new RedisList<Notification>(_redisConn, s_notificationKey, defaultExpiry);
            try
            {
                var result = await redis.RangeAsync();
                List<Notification> notis = result.ToList();
                return new Tuple<ErrorCode, List<Notification>>(ErrorCode.None, notis);
            }
            catch (Exception e)
            {
                // TODO: log
                _logger.LogError(e.Message);
                return new Tuple<ErrorCode, List<Notification>>(ErrorCode.RedisFailException, null);
            }
        }
        catch (Exception e)
        {
            // TODO: log
            _logger.LogError(e.Message);
            return new Tuple<ErrorCode, List<Notification>>(ErrorCode.RedisFailException, null);
        }
    }
}

