using DungeonAPI.Configs;
using DungeonAPI.RequestResponse;
using DungeonAPI.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Middleware;

public class CheckAuthAndVersion
{
    readonly RequestDelegate _next;
    readonly IMasterDataDb _masterData;
    readonly ILogger<CheckVersion> _logger;
    readonly IOptions<AppConfig> _appConfig;
    readonly IAuthUserDb _authUserDb;

    public CheckAuthAndVersion(RequestDelegate next, IMasterDataDb masterData,
        ILogger<CheckVersion> logger, IOptions<AppConfig> appConfig, IAuthUserDb authUserDb)
    {
        _next = next;
        _masterData = masterData;
        _logger = logger;
        _appConfig = appConfig;
        _authUserDb = authUserDb;
    }

    public async Task Invoke(HttpContext context)
    {
        var formString = context.Request.Path.Value;

        context.Request.EnableBuffering();

        if (string.Compare(formString, "/CreateAccount", StringComparison.OrdinalIgnoreCase) != 0)
        {
            await CheckVersion(context);
        }
        if (string.Compare(formString, "/Login", StringComparison.OrdinalIgnoreCase) != 0
            && string.Compare(formString, "/CreateNotice", StringComparison.OrdinalIgnoreCase) != 0)
        {
            await CheckAuth(context);
        }

        await _next(context);
    }

    async Task CheckAuth(HttpContext context)
    {
        context.Request.EnableBuffering();

        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            var bodyStr = await reader.ReadToEndAsync();

            var document = JsonDocument.Parse(bodyStr);

            try
            {
                var (isValid, authUser) = await IsValidPlayerThenLoadAuthPlayer(context, document);
                if (isValid)
                {
                    PushAuthUserToContextItem(context, authUser);
                    context.Request.Body.Position = 0;
                    await _next(context);
                }
                return;
            }
            catch (Exception e)
            {
                await SetResponseAuthFail(context, ErrorCode.AuthTokenFailException);
            }
            finally
            {
                context.Request.Body.Position = 0;
            }
        }
    }

    async Task<Tuple<bool, AuthUser>> IsValidPlayerThenLoadAuthPlayer(HttpContext context, JsonDocument document)
    {
        try
        {
            var email = document.RootElement.GetProperty("Email").GetString();
            var authToken = document.RootElement.GetProperty("AuthToken").GetString();
            var (LoadAuthUserErrorCode, authUser) = await _authUserDb.LoadAuthUserByEmail(email);

            if (LoadAuthUserErrorCode != ErrorCode.None)
            {
                await SetResponseAuthFail(context, LoadAuthUserErrorCode);
                return new Tuple<bool, AuthUser>(false, null);
            }
            return new Tuple<bool, AuthUser>(true, authUser);
        }
        catch (Exception e)
        {
            // TODO : log
            await SetResponseAuthFail(context, ErrorCode.AuthTockenFailException);
            return new Tuple<bool, AuthUser>(false, null);
        }

    }

    async Task SetResponseAuthFail(HttpContext context, ErrorCode errorCode)
    {
        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareRes
        {
            result = errorCode
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }

    void PushAuthUserToContextItem(HttpContext context, AuthUser authUser)
    {
        context.Items["PlayerId"] = authUser.PlayerId.ToString();
    }


    async Task CheckVersion(HttpContext context)
    {
        context.Request.EnableBuffering();

        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            try
            {
                var bodyStr = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(bodyStr) == true)
                {
                    // TODO: check - empty body 이면 아무것도 안함. 나중에 확인하기?
                    return;
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

