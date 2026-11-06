using MySqlConnector;
using TrabajaDecente.Api.Helpers;

namespace TrabajaDecente.Api.Middlewares;

/// <summary>
/// Traduce errores a respuestas JSON seguras: los mensajes de SIGNAL (45000) de la base son de negocio
/// y se muestran; cualquier otro error técnico se registra y se responde de forma genérica.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _log;
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log) { _next = next; _log = log; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (AppException ex) { await Write(ctx, ex.Status, ex.Message); }
        catch (BadHttpRequestException ex) { await Write(ctx, ex.StatusCode, "Solicitud no valida o demasiado grande"); }
        catch (Exception ex)
        {
            var my = FindMySql(ex);
            if (my is not null && my.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                await Write(ctx, 409, "Ya existe un registro con esos datos (correo, documento, NIT o duplicado)");
            else if (my is not null && my.SqlState == "45000")
                await Write(ctx, StatusFor(my.Message), my.Message);
            else
            {
                _log.LogError(ex, "Error no controlado en {Path}", ctx.Request.Path);
                await Write(ctx, 500, "Ocurrio un error inesperado. Intenta de nuevo mas tarde.");
            }
        }
    }

    private static int StatusFor(string message)
    {
        var m = message.ToLowerInvariant();
        if (m.Contains("no autorizado") || m.Contains("no esta aprobada")) return 403;
        if (m.Contains("no encontrad")) return 404;
        if (m.Contains("ya existe")) return 409;
        return 400;
    }

    private static MySqlException? FindMySql(Exception? ex)
    {
        while (ex is not null) { if (ex is MySqlException m) return m; ex = ex.InnerException; }
        return null;
    }

    private static async Task Write(HttpContext ctx, int status, string message)
    {
        if (ctx.Response.HasStarted) return;
        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(new { error = message });
    }
}
