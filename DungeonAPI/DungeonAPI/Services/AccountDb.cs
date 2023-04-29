﻿using System;
using System.Data;
using DungeonAPI;
using DungeonAPI.Models;
using DungeonAPI.ModelsDB;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;

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

        Open();

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, _compiler);
    }


    public async Task<ErrorCode> CreateAccountAsync(String email, String pw)
	{
        _logger.LogDebug($"Where: AccountDb.CreateAccount, Status: Try, Email: {email}");

        try
		{
            // 계정 중복 확인
            var accountInfo = await _queryFactory.Query("account").Where("Email", email).FirstOrDefaultAsync<Account>();
            if (accountInfo != null && accountInfo.Email == email)
            {
                _logger.LogDebug($"Where: AccountDb.CreateAccount, Status: {ErrorCode.CreateAccountFailDuplicatedEmail}, Email: {email}");
                return ErrorCode.CreateAccountFailDuplicatedEmail;
            }
            
		    // 솔트값, 해시pw값 설정
		    String saltValue = Security.MakeSaltString();
		    String hashedPassword = Security.MakeHashingPassWord(saltValue, pw);

            // id, 솔티값, 해시pw값 db에 저장
            var count = await _queryFactory.Query("account").InsertAsync(new {
                                                            Email = email,
                                                            SaltValue = saltValue,
                                                            HashedPassword = hashedPassword
                                                            });
            _logger.LogDebug(
                $"Where: AccountDb.CreateAccount, Status: InsertToDb, Email: {email}, SaltValue: {saltValue}, hashedPassword:{hashedPassword}");

            if (count != 1)
            {
                return ErrorCode.CreateAccountFailInsert;
            }
		    return ErrorCode.None;
        }
		catch (Exception e)
		{
            _logger.LogError(e,
                $"Where: AccountDb.CreateAccount, Status: Error, ErrorCode: {ErrorCode.CreateAccountFailException}, Email: {email}");
            return ErrorCode.CreateAccountFailException;
        }
        finally
        {
            Dispose();
        }
	}

    public async Task<ErrorCode> VerifyAccountAsync(String email, String pw)
    {
        _logger.LogDebug($"Where: AccountDb.VerifyAccountAsync, Status: Try, Email: {email}");
        //email의 salt값, hashedPW 가져오기
        try
        {
            var accountInfo = await _queryFactory.Query("account").Where("Email", email).FirstOrDefaultAsync<Account>();
            if (accountInfo == null)
            {
                _logger.LogDebug($"Where: AccountDb.VerifyAccountAsync, Status: {ErrorCode.LoginFailUserNotExist}, Email: {email}");
                return ErrorCode.LoginFailUserNotExist;
            }

            //hashing한 pw랑 일치하는지 비교하기
            String HashedPassword = Security.MakeHashingPassWord(accountInfo.SaltValue, pw);
            if (accountInfo.HashedPassword != HashedPassword)
            {
                _logger.LogDebug($"Where: AccountDb.VerifyAccountAsync, Status: {ErrorCode.LoginFailPwNotMatch}, Email: {email}");
                return ErrorCode.LoginFailPwNotMatch;
            }

            // 정상이면 ErrorNode.None 리턴받아서 다음 동작 진행할 수 있게 하\
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                $"Where: AccountDb.VerifyAccount, Status: Error, ErrorCode: {ErrorCode.LoginFailException}, Email: {email}");
            return ErrorCode.LoginFailException;
        }
        finally
        {
            Dispose();
        }
    }

    public void Dispose()
	{
        _dbConn.Close();
    }


    private void Open()
    {
        _dbConn = new MySqlConnection(_dbConfig.Value.AccountDb);

        _dbConn.Open();
    }

    private void Close()
    {
        _dbConn.Close();
    }
}

