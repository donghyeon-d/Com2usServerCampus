using System;
using System.Data;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata;
using SqlKata.Execution;
using ZLogger;
using DungeonAPI.Models;
using static DungeonAPI.Models.MasterData;

namespace DungeonAPI.Services;

public class MasterDataDb : IMasterDataDb
{
    readonly ILogger<MasterDataDb> _logger;
    readonly IOptions<DbConfig> _dbConfig;

    IDbConnection _dbConn;
    SqlKata.Compilers.MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public static MasterData s_masterData = new MasterData();

    public MasterDataDb(ILogger<MasterDataDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;
    }

    public async Task<Tuple<ErrorCode, IEnumerable<MasterData.Item>>> Load()
    {
        Open();

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, _compiler);

        try
        {
            var item = _queryFactory.Query("Item").Get<MasterData.Item>();
            s_masterData._item = item;
            await Console.Out.WriteLineAsync(item.ToString());
            return new Tuple<ErrorCode, IEnumerable<MasterData.Item>>(ErrorCode.None, item);
        }
        catch
        {
            return new Tuple<ErrorCode, IEnumerable<MasterData.Item>>(ErrorCode.GetMasterDataDBConnectionFail, null);
        }
        finally
        {
            Close();
        }

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

