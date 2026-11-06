namespace TrabajaDecente.Api.Middlewares;

public class SecurityHeadersMiddleware
{
    // Sin scripts en línea. Bootstrap/Tailwind/iconos/fuentes vienen de CDN; en producción conviene alojarlos localmente.
    private const string Csp =
        "default-src 'self'; " +
        "script-src 'self' https://cdn.jsdelivr.net https://cdn.tailwindcss.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
        "font-src 'self' https://cdn.jsdelivr.net https://fonts.gstatic.com; " +
        "img-src 'self' data:; connect-src 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

    private readonly RequestDelegate _next;
    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext ctx)
    {
        var h = ctx.Response.Headers;
        h["X-Content-Type-Options"] = "nosniff";
        h["X-Frame-Options"] = "DENY";
        h["Referrer-Policy"] = "no-referrer";
        h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        h["Content-Security-Policy"] = Csp;
        if (ctx.Request.Path.StartsWithSegments("/api")) h["Cache-Control"] = "no-store";
        return _next(ctx);
    }
}
