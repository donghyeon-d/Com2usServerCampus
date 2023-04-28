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

    public static MasterData? s_Data = null; // readonly 로 하고싶

    public MasterDataDb(ILogger<MasterDataDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        if (s_Data == null)
        {
            s_Data = new MasterData();
            LoadFromDb();
        }
    }

    //public async Task<Tuple<ErrorCode, IEnumerable<MasterData.Item>>> Load()
    //{
    //    Open();

    //    _compiler = new SqlKata.Compilers.MySqlCompiler();
    //    _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, _compiler);

    //    try
    //    {
    //        var item = _queryFactory.Query("Item").Get<MasterData.Item>();
    //        s_Data._item = item;
    //        await Console.Out.WriteLineAsync(item.ToString());
    //        return new Tuple<ErrorCode, IEnumerable<MasterData.Item>>(ErrorCode.None, item);
    //    }
    //    catch
    //    {
    //        return new Tuple<ErrorCode, IEnumerable<MasterData.Item>>(ErrorCode.GetMasterDataDBConnectionFail, null);
    //    }
    //    finally
    //    {
    //        Close();
    //    }
    //}

    public async Task<Tuple<ErrorCode, MasterData>> Get()
    {
        Open();

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, _compiler);

        try
        {
            if (s_Data == null)
            {
                LoadFromDb();
            }
            return new Tuple<ErrorCode, MasterData>(ErrorCode.None, s_Data);
        }
        catch
        {
            _logger.LogDebug("Masterdata Load Fail");
            return new Tuple<ErrorCode, MasterData>(ErrorCode.GetMasterDataDBConnectionFail, null);
        }
        finally
        {
            Close();
        }
    }


    private void LoadFromDb()
    {
        Open();

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, _compiler);

        try
        {
            if (s_Data != null)
            {
                s_Data._item = _queryFactory.Query("Item").Get<MasterData.Item>();
                s_Data._itemAttribute = _queryFactory.Query("ItemAttribute").Get<MasterData.ItemAttribute>();
                s_Data._attendanceReward = _queryFactory.Query("AttendanceReward").Get<MasterData.AttendanceReward>();
                s_Data._inAppProduct = _queryFactory.Query("InAppProduct").Get<MasterData.InAppProduct>();
                s_Data._stageItem = _queryFactory.Query("StageItem").Get<MasterData.StageItem>();
                s_Data._stageAttackNPC = _queryFactory.Query("StageAttackNPC").Get<MasterData.StageAttackNPC>();
            }
        }
        catch
        {
            _logger.LogDebug("Masterdata Load Fail");
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

