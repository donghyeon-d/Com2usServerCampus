using DungeonAPI.ModelDB;
using CloudStructures.Structures;
using ZLogger;

namespace DungeonAPI.Services;

public partial class MemoryDb : IMemoryDb
{
    static string s_notificationKey { get; set; } = Util.KeyMaker.MakeNoticeKey();
    readonly TimeSpan noticeDefaultExpiry = TimeSpan.FromDays(1);

    public async Task<ErrorCode> CreateNotification(string title, string content, DateTime? dateTime = null)
    {
        try
        {
            var redis = new RedisList<Notification>(_redisConn, s_notificationKey, noticeDefaultExpiry);
            try
            {
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
                _logger.ZLogWarning(e.Message);
                return ErrorCode.RedisFailException;
            }
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.RedisFailException;
        }
    }

    public async Task<Tuple<ErrorCode, List<Notification>?>> ReadNotificationList()
    {
        try
        {
            var redis = new RedisList<Notification>(_redisConn, s_notificationKey, noticeDefaultExpiry);
            try
            {
                var result = await redis.RangeAsync();
                List<Notification> notis = result.ToList();
                return new (ErrorCode.None, notis);
            }
            catch (Exception e)
            {
                _logger.ZLogWarning(e.Message);
                return new (ErrorCode.RedisFailException, null);
            }
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new (ErrorCode.RedisFailException, null);
        }
    }
}

