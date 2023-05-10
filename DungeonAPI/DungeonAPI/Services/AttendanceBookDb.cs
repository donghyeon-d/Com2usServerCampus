using System;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using static Humanizer.On;

namespace DungeonAPI.Services;

public class AttendanceBookDb : GameDb, IAttendanceBookDb
{
    readonly ILogger<AttendanceBookDb> _logger;
    readonly IMailDb _mailDb;
    readonly IMasterDataDb _masterDataDb;
    int _expiredDate = 30;

    public AttendanceBookDb(ILogger<AttendanceBookDb> logger, IOptions<DbConfig> dbConfig,
        IMailDb mailDb, IMasterDataDb masterDataDb)
        : base(logger, dbConfig)
    {
        _logger = logger;
        _mailDb = mailDb;
        _masterDataDb = masterDataDb;
    }

    public async Task<Tuple<ErrorCode, AttendanceBook>> LoadAttandanceBookInfoByPlayerId(Int32 playerId)
    {
        Open();
        try
        {
            var result = await _queryFactory.Query("AttendanceBook")
                                           .Where("PlayerId", playerId)
                                           .FirstOrDefaultAsync<AttendanceBook>();
            if (result == null)
            {
                return new Tuple<ErrorCode, AttendanceBook>(ErrorCode.LoadPlayerAttendanceBookNotExist, null);
            }

            return new Tuple<ErrorCode, AttendanceBook>(ErrorCode.None, result);
        }
        catch (Exception e)
        {
            // TODO: log
            return new Tuple<ErrorCode, AttendanceBook>(ErrorCode.LoadPlayerAttendanceBookFailException, null);
        }
        finally
        {
            Dispose();
        }
    }

    public async Task<ErrorCode> ReceiveRewardToMail(Int32 playerId)
    {
        Open();
        try
        {
            var playerAttendanceBook = await _queryFactory.Query("AttendanceBook")
                                           .Where("PlayerId", playerId)
                                           .FirstOrDefaultAsync<AttendanceBook>();
            if (playerAttendanceBook == null)
            {
                return ErrorCode.LoadPlayerAttendanceBookNotExist;
            }

            AttendanceBook UpdateAttendanceBook = new AttendanceBook { PlayerId = playerAttendanceBook.PlayerId };

            if (CanReceiveAttendanceReward(playerAttendanceBook) == false)
            {
                return ErrorCode.AlreadyReceiveAttendanceReward;
            }

            if (IsConsecute(playerAttendanceBook))
            {
                UpdateAttendanceBook.StartDate = playerAttendanceBook.StartDate;
                UpdateAttendanceBook.LastReceiveDate = DateTime.Today;
                UpdateAttendanceBook.ConsecutiveDays = playerAttendanceBook.ConsecutiveDays + 1;
            }
            else
            {
                UpdateAttendanceBook.StartDate = DateTime.Today;
                UpdateAttendanceBook.LastReceiveDate = DateTime.Today;
                UpdateAttendanceBook.ConsecutiveDays = 1;
            }

            ErrorCode updateAttendanceBookErrorCode = await this.UpdateAttendanceBook(playerId, UpdateAttendanceBook);
            if (updateAttendanceBookErrorCode != ErrorCode.None)
            {
                return updateAttendanceBookErrorCode;
            }

            ErrorCode SendToMailErrorCode = await SendToMail(UpdateAttendanceBook);
            if (SendToMailErrorCode != ErrorCode.None)
            {
                await RollbackAttendanceBook(playerId, playerAttendanceBook);
                return SendToMailErrorCode;
            }

            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.ReceiveRewardToMailFailException;
        }
        finally
        {
            Dispose();
        }
    }

    bool IsConsecute(AttendanceBook playerAttendanceBook)
    {
        if (playerAttendanceBook.LastReceiveDate.Date == DateTime.Today.AddDays(-1).Date)
        {
            return true;
        }
        return false;
    }

    async Task<ErrorCode> UpdateAttendanceBook(Int32 playerId, AttendanceBook attendanceBook)
    {
        try
        {
            int updateResult = await _queryFactory.Query("AttendanceBook")
                                            .Where("PlayerId", playerId)
                                            .UpdateAsync(attendanceBook);
            if (updateResult != 1)
            {
                return ErrorCode.UpdateAttendanceBookTupleFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.UpdateAttendanceBookTupleFailException;
        }
    }

    async Task<ErrorCode> RollbackAttendanceBook(Int32 playerId, AttendanceBook attendanceBook)
    {
        return await UpdateAttendanceBook(playerId, attendanceBook);
    }

    async Task<ErrorCode> SendToMail(AttendanceBook attendanceBook)
    {
        // TODO : Mail() 생성자로 만들기
        Mail mail = new Mail {
            PlayerId = attendanceBook.PlayerId,
            Title = "AttendanceBook Reward",
            PostDate = attendanceBook.LastReceiveDate,
            ExpiredDate = attendanceBook.LastReceiveDate.AddDays(_expiredDate),
            IsOpened = 0,
            IsReceivedReward = 0,
            IsDeleted = 0,
            CanDelete = 1,
            Sender = "AttendanceBook"
        };

        MailContent mailContent = new MailContent {
            Content = $"Congratulations! You have attended for " +
            $"{attendanceBook.ConsecutiveDays} consecutive days."
        };

        List<MailReward> mailRewards = new List<MailReward>();
        var rewards = MasterDataDb.s_attendanceReward
                    .FindAll(item => item.Day == attendanceBook.ConsecutiveDays);
        foreach (MasterData.AttendanceReward reward in rewards)
        {
            mailRewards.Add(new MailReward
            {
                BaseItemCode = reward.ItemCode,
                ItemCount = reward.Count,
                IsReceived = false
            }) ;
        }

        try
        {
            var (createMailErrorCode, mailId) = await _mailDb.CreateMail(mail, mailContent, mailRewards);
            if (createMailErrorCode != ErrorCode.None)
            {
                return createMailErrorCode;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.SendToMailExceptionAtReceiveAttendanceReward;
        }
    }

    //계정 생성하고 기본 데이터 생성할 때 생성하기
    public async Task<ErrorCode> CreatePlayerAttendanceBook(Int32 playerId)
    {
        Open();
        try
        {
            int count = await _queryFactory.Query("AttendanceBook").InsertAsync(new
            {
                PlayerId = playerId,
                StartDate = DateTime.Today,
                LastReceiveDate = DateTime.Today.AddDays(-10),
                ConsecutiveDays = 0
            });

            if (count != 1)
            {
                return ErrorCode.CreatePlayerAttendanceBookFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            return ErrorCode.CreatePlayerAttendanceBookFailException;
        }
        finally
        {
            Dispose();
        }
    }

    public async Task<ErrorCode> DeletePlayerAttendanceBook(Int32 playerId)
    {
        Open();
        try
        {
            int count = await _queryFactory.Query("AttendanceBook")
                                            .Where("PlayerId", playerId)
                                            .DeleteAsync();
            if (count != 1)
            {
                return ErrorCode.DeletePlayerAttendanceBookFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            // TODO : log
            return ErrorCode.DeletePlayerAttendanceBookFailException;
        }
        finally
        {
            Dispose();
        }
    }

    public Int32 GetConsecutiveDays(AttendanceBook attendanceBook)
    {
        if (attendanceBook.StartDate.Date == attendanceBook.LastReceiveDate.Date)
        {
            return attendanceBook.ConsecutiveDays;
        }
        if (attendanceBook.StartDate.Date == attendanceBook.LastReceiveDate.AddDays(-1).Date)
        {
            return attendanceBook.ConsecutiveDays + 1;
        }
        return 1;
    }

    public bool CanReceiveAttendanceReward(AttendanceBook attendanceBook)
    {
        if (attendanceBook.LastReceiveDate.Date == DateTime.Today.Date)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}

