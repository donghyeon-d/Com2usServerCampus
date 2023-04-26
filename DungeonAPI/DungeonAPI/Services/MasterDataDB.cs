using System;
using System.Data;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata;
using SqlKata.Execution;
using ZLogger;

namespace DungeonAPI.Services;

public class MasterDataDB : IMasterDataDb
{
    readonly ILogger<MasterDataDB> _logger;
    readonly IOptions<DbConfig> _dbConfig;

    IDbConnection _dbConn;
    SqlKata.Compilers.MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public MasterDataDB(ILogger<MasterDataDB> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;
    }

    public Task<ErrorCode> Load()
    {
        try
        {
            Open();
        }
        catch
        {
            return ErrorCode.GetMasterDataDBConnectionFail;
        }

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, _compiler);


        return ErrorCode.None;
    }

    private void Open()
    {
        _dbConn = new MySqlConnection(_dbConfig.Value.MasterDataDb);

        _dbConn.Open();
    }

    private void Close()
    {
        _dbConn.Close();
    }
}

