using System;
using Microsoft.Extensions.Options;
using System.Data;
using SqlKata.Execution;
using MySqlConnector;
using DungeonAPI.Configs;

namespace DungeonAPI.Services;

public class GameDb
{
    readonly ILogger<GameDb> _logger;
    readonly IOptions<DbConfig> _dbConfig;

    IDbConnection _dbConn;
    SqlKata.Compilers.MySqlCompiler _compiler;
    protected QueryFactory _queryFactory;

    public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        _dbConn = new MySqlConnection(_dbConfig.Value.GameDb);

        Open();
        
        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConn, _compiler);
    }

    protected void Open()
    {
        if (_dbConn.State == ConnectionState.Closed)
        {
            _dbConn.Open();
        }
    }

    void Close()
    {
        _dbConn.Close();
    }

    protected void Dispose()
    {
        Close();
    }

}

