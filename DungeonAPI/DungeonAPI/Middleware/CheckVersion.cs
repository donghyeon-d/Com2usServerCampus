using System;
using DungeonAPI.Services;
using DungeonAPI.Configs;
using DungeonAPI.MessageBody;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace DungeonAPI.Middleware;

public class CheckVersion
{
    readonly RequestDelegate _next;
    readonly IMasterDataDb _masterData;
    readonly ILogger<CheckVersion> _logger;
    readonly IOptions<AppConfig> _appConfig;

    public CheckVersion(RequestDelegate next, IMasterDataDb masterData, ILogger<CheckVersion> logger, IOptions<AppConfig> appConfig)
	{
        _next = next;
        _masterData = masterData;
        _logger = logger;
        _appConfig = appConfig;
	}

    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            var bodyStr = await reader.ReadToEndAsync();

            var document = JsonDocument.Parse(bodyStr);

            if (await IsValidAppVersion(context, document))
            {
                await IsValidMasterDataVersion(context, document);
            }
        }

        await _next(context);
    }

    //    bool IsValidVersion(HttpContext context)
    //    {
    //        context.Request.Body.
    //    }
    async Task<bool> IsValidAppVersion(HttpContext context, JsonDocument document)
    {
        try
        {
            var version = document.RootElement.GetProperty("AppVersion").GetString();
            if (version != _appConfig.Value.ToString())
            {
                await SetResponseInvalidAppVersion(context);
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            //TODO : log
            await SetResponseInvalidAppVersion(context);
            return false;
        }
    }

    async Task SetResponseInvalidAppVersion(HttpContext context)
    {
        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareRes
        {
            result = ErrorCode.InValidRequestAppVersion
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }

    async Task<bool> IsValidMasterDataVersion(HttpContext context, JsonDocument document)
    {
        try
        {
            var version = document.RootElement.GetProperty("MasterDataVersion").GetString();
            if (version != MasterDataDb.s_meta.First().ToString()) // TODO : check
            {
                await SetResponseInvalidMasterDataVersion(context);
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            //TODO : log
            await SetResponseInvalidMasterDataVersion(context);
            return false;
        }
    }

    async Task SetResponseInvalidMasterDataVersion(HttpContext context)
    {
        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareRes
        {
            result = ErrorCode.InValidRequestMasterDataVersion
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
}

