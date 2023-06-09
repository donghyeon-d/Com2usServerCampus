﻿using System.Data;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using DungeonAPI.ModelDB;
using static DungeonAPI.ModelDB.MasterData;
using DungeonAPI.Configs;
using ZLogger;

namespace DungeonAPI.Services;

public class MasterDataDb : IMasterDataDb
{
    readonly ILogger<MasterDataDb> _logger;
    readonly IOptions<DbConfig> _dbConfig;

    IDbConnection _dbConn;
    SqlKata.Compilers.MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public static List<Meta>? s_meta { get; set; } = null;
    public static List<MasterData.BaseItem>? s_baseItem { get; set; } = null;
    public static List<MasterData.ItemAttribute>? s_itemAttribute { get; set; } = null;
    public static List<AttendanceReward>? s_attendanceReward { get; set; } = null;
    public static List<InAppProduct>? s_inAppProduct { get; set; } = null;
    public static List<DungeonStage>? s_stage { get; set; } = null;
    public static List<StageItem>? s_stageItem { get; set; } = null;
    public static List<StageAttackNPC>? s_stageAttackNPC { get; set; } = null;

    public MasterDataDb(ILogger<MasterDataDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        _dbConn = new MySqlConnection(_dbConfig.Value.MasterDataDb);
        Open();
        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConn, _compiler);
    }

    public async Task Init()
    {
        await LoadFromDb();
    }

    private async Task LoadFromDb()
    {
        try
        {
            
            var meta = await _queryFactory.Query("Meta").GetAsync<MasterData.Meta>();
            s_meta = meta.ToList();
            var item = await _queryFactory.Query("BaseItem").GetAsync<MasterData.BaseItem>();
            s_baseItem = item.ToList();
            var itemAttribute = await _queryFactory.Query("ItemAttribute").GetAsync<MasterData.ItemAttribute>();
            s_itemAttribute = itemAttribute.ToList();
            var attendanceReward = await _queryFactory.Query("AttendanceReward").GetAsync<MasterData.AttendanceReward>();
            s_attendanceReward = attendanceReward.ToList();
            var inAppProduct = await _queryFactory.Query("InAppProduct").GetAsync<MasterData.InAppProduct>();
            s_inAppProduct = inAppProduct.ToList();
            var stage = await _queryFactory.Query("DungeonStage").GetAsync<MasterData.DungeonStage>();
            s_stage = stage.ToList();
            var stageItem = await _queryFactory.Query("StageItem").GetAsync<MasterData.StageItem>();
            s_stageItem = stageItem.ToList();
            var stageAttackNPC = await _queryFactory.Query("StageAttackNPC").GetAsync<MasterData.StageAttackNPC>();
            s_stageAttackNPC = stageAttackNPC.ToList();
        }
        catch (Exception e)
        {
            _logger.ZLogWarning(e.Message);
        }
        finally
        {
            Close();
        }
    }

    private void Open()
    {
        _dbConn.Open();
    }

    private void Close()
    {
        _dbConn.Close();
    }
}

