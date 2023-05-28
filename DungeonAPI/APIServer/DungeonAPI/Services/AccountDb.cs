using System;
using System.Data;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using DungeonAPI.ModelsDB;
using DungeonAPI.Util;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using ZLogger;

namespace DungeonAPI.Services;

public class AccountDb : IAccountDb
{

    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<AccountDb> _logger;

    IDbConnection _dbConn;
    SqlKata.Compilers.MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public AccountDb(ILogger<AccountDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        _dbConn = new MySqlConnection(_dbConfig.Value.AccountDb);

        Open();

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, _compiler);
    }


    public async Task<Tuple<ErrorCode, Int32>> CreateAccountAsync(String email, String pw)
    {
        try
        {
            var accountInfo = await _queryFactory.Query("Account").Where("Email", email).FirstOrDefaultAsync<Account>();
            if (accountInfo != null && accountInfo.Email == email)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.CreateAccountFailDuplicatedEmail, -1);
            }

            String saltValue = Security.MakeSaltString();
            String hashedPassword = Security.MakeHashingPassWord(saltValue, pw);

            var accountId = await _queryFactory.Query("Account").InsertGetIdAsync<Int32>(new {
                Email = email,
                SaltValue = saltValue,
                HashedPassword = hashedPassword,
                IsDeleted = 0
            });

            if (accountId == -1)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.CreateAccountFailInsert, -1);
            }
            return new Tuple<ErrorCode, Int32>(ErrorCode.None, accountId);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new Tuple<ErrorCode, Int32>(ErrorCode.CreateAccountFailException, -1);
        }
    }

    public async Task<Tuple<ErrorCode, Int32>> VerifyAccountAsync(String email, String pw)
    {
        try
        {
            var accountInfo = await _queryFactory.Query("Account").Where("Email", email).FirstOrDefaultAsync<Account>();
            if (accountInfo == null)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.LoginFailPlayerNotExist, 0);
            }

            String HashedPassword = Security.MakeHashingPassWord(accountInfo.SaltValue, pw);
            if (accountInfo.HashedPassword != HashedPassword)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.LoginFailPwNotMatch, 0);
            }

            return new Tuple<ErrorCode, Int32>(ErrorCode.None, accountInfo.AccountId);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new Tuple<ErrorCode, Int32>(ErrorCode.LoginFailException, 0);
        }
    }

    public async Task<ErrorCode> DeleteAccountAsync(String email)
    {
        try
        {
            int count = await _queryFactory.Query("Account")
                                            .Where("Email", email)
                                            .DeleteAsync();
            if (count != 1)
            {
                return ErrorCode.DeleteAccountFail;
            }
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return ErrorCode.DeleteAccountFailException;
        }
    }

    public async Task<Tuple<ErrorCode, Int32>> LoadAccountIdByEmail(String email)
    {
        try
        {
            var account = await _queryFactory.Query("Account")
                                            .Where("Email", email)
                                            .FirstOrDefaultAsync<Account>();
            if (account is null)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.LoadAccountEmailNotMatch, -1);
            }
            return new Tuple <ErrorCode, Int32>(ErrorCode.None, account.AccountId);
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
            return new Tuple<ErrorCode, Int32>(ErrorCode.LoadAccountFailException, -1);
        }
    }

    public void Dispose()
    {
        _dbConn.Close();
    }

    void Open()
    {
        if (_dbConn.State == System.Data.ConnectionState.Closed)
        {
            _dbConn.Open();
        }
    }

    void Close()
    {
        _dbConn.Close();
    }
}
