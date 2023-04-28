using System;
using System.Data;
using DungeonAPI;
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
		try
		{
            // 계정 중복 확인
            var accountInfo = await _queryFactory.Query("account").Where("Email", email).FirstOrDefaultAsync<Account>();
            if (accountInfo != null && accountInfo.Email == email)
            {
                _logger.LogDebug($"[CreateAccount] DuplicatedEmail: {email}");
                return ErrorCode.CreateAccountFailDuplicatedEmail;
            }
            
		    // 솔트값, 해시pw값 설정
		    String saltValue = Security.MakeSaltString();
		    String hashedPassword = Security.MakeHashingPassWord(saltValue, pw);
            _logger.LogDebug(
                $"[CreateAccount] Email: {email}, SaltValue: {saltValue}, hashedPassword:{hashedPassword}");

            // id, 솔티값, 해시pw값 db에 저장
            var count = await _queryFactory.Query("account").InsertAsync(new {
                                                            Email = email,
                                                            SaltValue = saltValue,
                                                            HashedPassword = hashedPassword
                                                            });

            if (count != 1)
            {
                return ErrorCode.CreateAccountFailInsert;
            }
		    return ErrorCode.None;
        }
		catch (Exception e)
		{
            _logger.LogError(e,
                $"[AccountDb.CreateAccount] ErrorCode: {ErrorCode.CreateAccountFailException}, Email: {email}");
            return ErrorCode.CreateAccountFailException;
        }
	}

    //public Task<Tuple<ErrorCode, Int64>> VerifyAccount(String email, String pw)
	//{
        // email의 salt값, hashedPW 가져오기
        // hashing한 pw랑 일치하는지 비교하기

	//}

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

