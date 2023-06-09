﻿using DungeonAPI.Configs;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using DungeonAPI.ModelDB;
using ZLogger;

namespace DungeonAPI.Middleware;

public class CheckAuthAndVersion
{
    readonly RequestDelegate _next;
    readonly ILogger<CheckAuthAndVersion> _logger;
    readonly IOptions<AppConfig> _appConfig;
    readonly IMemoryDb _memoryDb;

    public CheckAuthAndVersion(RequestDelegate next, 
        ILogger<CheckAuthAndVersion> logger, IOptions<AppConfig> appConfig, IMemoryDb memoryDb)
    {
        _next = next;
        _logger = logger;
        _appConfig = appConfig;
        _memoryDb = memoryDb;
    }

    public async Task Invoke(HttpContext context)
    {
        var formString = context.Request.Path.Value;

        if (string.Compare(formString, "/Login", StringComparison.OrdinalIgnoreCase) == 0
            || string.Compare(formString, "/CreateNotice", StringComparison.OrdinalIgnoreCase) == 0
            || string.Compare(formString, "/CreateAccount", StringComparison.OrdinalIgnoreCase) == 0)
        {
            await _next(context);

            return;
        }

        if (await CheckVersion(context) == false || await CheckAuth(context) == false)
        {
            return;
        }

        await _next(context);
    }

    async Task<bool> CheckAuth(HttpContext context)
    {
        context.Request.EnableBuffering();

        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            var bodyStr = await reader.ReadToEndAsync();

            var document = JsonDocument.Parse(bodyStr);

            try
            {
                var (isValid, playerInfo) = await IsValidPlayerThenLoadAuthPlayer(context, document);
                if (isValid && playerInfo is not null)
                {
                    PushPlayerInfoToContextItem(context, playerInfo);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                _logger.ZLogInformation(e.Message);
                await SetResponseAuthFail(context, ErrorCode.AuthTokenFailException);
                return false;
            }
            finally
            {
                context.Request.Body.Position = 0;
            }
        }
    }

    async Task<Tuple<bool, PlayerInfo?>> IsValidPlayerThenLoadAuthPlayer(HttpContext context, JsonDocument document)
    {
        try
        {
            var playerId = document.RootElement.GetProperty("PlayerId").GetInt32();
            var authToken = document.RootElement.GetProperty("AuthToken").GetString();
            // playerId는 request에서 required로 정의되어 있기에 null일 수 없음
            var (LoadAuthUserErrorCode, authUser) = await _memoryDb.LoadPlayer(playerId);

            if (LoadAuthUserErrorCode != ErrorCode.None)
            {
                await SetResponseAuthFail(context, LoadAuthUserErrorCode);
                return new (false, null);
            }
            return new (true, authUser);
        }
        catch (Exception e)
        {

            _logger.ZLogInformation(e.Message);
            await SetResponseAuthFail(context, ErrorCode.AuthTockenFailException);
            return new (false, null);
        }

    }

    async Task SetResponseAuthFail(HttpContext context, ErrorCode errorCode)
    {
        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareRes
        {
            Result = errorCode
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }

    void PushPlayerInfoToContextItem(HttpContext context, PlayerInfo playerInfo)
    {
        context.Items["PlayerInfo"] = playerInfo;
    }

    async Task<bool> CheckVersion(HttpContext context)
    {
        context.Request.EnableBuffering();

        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            try
            {
                var bodyStr = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(bodyStr) == true)
                {
                    return false;
                }

                var document = JsonDocument.Parse(bodyStr);

                if (await IsValidAppVersion(context, document) == true &&
                    await IsValidMasterDataVersion(context, document) == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                _logger.ZLogInformation(e.Message);
                await SetResponseEmptyRequestBody(context);
                return false;
            }
            finally
            {
                context.Request.Body.Position = 0;
            }
        }
    }

    async Task<bool> IsValidAppVersion(HttpContext context, JsonDocument document)
    {
        try
        {
            var version = document.RootElement.GetProperty("AppVersion").GetString();
            if (version != _appConfig.Value.Version.ToString())
            {
                await SetResponseInvalidAppVersion(context);
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.ZLogInformation(e.Message);
            await SetResponseInvalidAppVersion(context);
            return false;
        }
    }

    async Task SetResponseInvalidAppVersion(HttpContext context)
    {
        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareRes
        {
            Result = ErrorCode.InValidRequestAppVersion
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }

    async Task<bool> IsValidMasterDataVersion(HttpContext context, JsonDocument document)
    {
        try
        {
            var a = MasterDataDb.s_meta;
            var version = document.RootElement.GetProperty("MasterDataVersion").GetString();
            // Program.cs에서 MasterDataDb.Init() 으로 s_meta를 로드해오기 때문에 null일 수 없음
            if (version != MasterDataDb.s_meta.Last().Version.ToString())
            {
                await SetResponseInvalidMasterDataVersion(context);
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.ZLogInformation(e.Message);
            await SetResponseInvalidMasterDataVersion(context);
            return false;
        }
    }

    async Task SetResponseInvalidMasterDataVersion(HttpContext context)
    {
        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareRes
        {
            Result = ErrorCode.InValidRequestMasterDataVersion
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }

    async Task SetResponseEmptyRequestBody(HttpContext context)
    {
        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareRes
        {
            Result = ErrorCode.EmptyRequestBody
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }


}

