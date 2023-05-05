using System;
using System.Text;
using System.Text.Json;
using DungeonAPI.Configs;
using DungeonAPI.MessageBody;
using DungeonAPI.Services;
using DungeonAPI.ModelDB;
using Microsoft.Extensions.Options;

namespace DungeonAPI.Middleware;

public class CheckAuth
{
    readonly RequestDelegate _next;
    readonly ILogger<CheckAuth> _logger;
    readonly IAuthUserDb _authUserDb;

    public CheckAuth(RequestDelegate next, ILogger<CheckAuth> logger,
        IAuthUserDb authUserDb)
    {
        _next = next;
        _logger = logger;
        _authUserDb = authUserDb;
    }

    public async Task Invoke(HttpContext context)
    {
        var formString = context.Request.Path.Value;
        if (string.Compare(formString, "/CreateAccount", StringComparison.OrdinalIgnoreCase) == 0
            || string.Compare(formString, "/Login", StringComparison.OrdinalIgnoreCase) == 0
            || string.Compare(formString, "/CreateNotice", StringComparison.OrdinalIgnoreCase) == 0)
        {
            await _next(context);

            return;
        }

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

    async Task<Tuple <bool, AuthUser>> IsValidPlayerThenLoadAuthPlayer(HttpContext context, JsonDocument document)
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
}
