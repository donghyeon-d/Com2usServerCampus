using System;
using DungeonAPI.Services;
using DungeonAPI.Configs;
using DungeonAPI.RequestResponse;
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

    public CheckVersion(RequestDelegate next, IMasterDataDb masterData,
        ILogger<CheckVersion> logger, IOptions<AppConfig> appConfig)
	{
        _next = next;
        _masterData = masterData;
        _logger = logger;
        _appConfig = appConfig;
	}

    public async Task Invoke(HttpContext context)
    {
        var formString = context.Request.Path.Value;
        if (string.Compare(formString, "/CreateAccount", StringComparison.OrdinalIgnoreCase) == 0)
        {
            // Call the next delegate/middleware in the pipeline
            await _next(context);

            return;
        }

        context.Request.EnableBuffering();

        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            try
            {
                var bodyStr = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(bodyStr) == true)
                {
                    // TODO: check - empty body 이면 아무것도 안함. 나중에 확인하기?
                    return ;
                }

                var document = JsonDocument.Parse(bodyStr);

                if (await IsValidAppVersion(context, document))
                {
                    if (await IsValidMasterDataVersion(context, document))
                    {
                        context.Request.Body.Position = 0;
                        await _next(context);
                    }    
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                await SetResponseEmptyRequestBody(context);
                return;
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
            if (version != MasterDataDb.s_meta.Last().Version.ToString()) // TODO : check
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

    async Task SetResponseEmptyRequestBody(HttpContext context)
    {
        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareRes
        {
            result = ErrorCode.EmptyRequestBody
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
}

