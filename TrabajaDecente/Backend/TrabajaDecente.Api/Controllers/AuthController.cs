using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;
using TrabajaDecente.Api.Services;

namespace TrabajaDecente.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string CookieName = "td_refresh";
    private readonly IAuthService _auth;
    private readonly IWebHostEnvironment _env;
    public AuthController(IAuthService auth, IWebHostEnvironment env) { _auth = auth; _env = env; }

    [HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var r = await _auth.LoginAsync(req);
        SetCookie(r.RefreshToken, r.RefreshExpires);
        return r.Response;
    }

    [HttpPost("registro/estudiante"), EnableRateLimiting("auth")]
    public async Task<ActionResult<MensajeResponse>> RegistrarEstudiante(RegistroEstudianteRequest req) =>
        StatusCode(201, await _auth.RegistrarEstudianteAsync(req));

    [HttpPost("registro/empresa"), EnableRateLimiting("auth")]
    public async Task<ActionResult<MensajeResponse>> RegistrarEmpresa(RegistroEmpresaRequest req) =>
        StatusCode(201, await _auth.RegistrarEmpresaAsync(req));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        RequireAjax();
        try
        {
            var r = await _auth.RefreshAsync(Request.Cookies[CookieName]);
            SetCookie(r.RefreshToken, r.RefreshExpires);
            return r.Response;
        }
        catch (AppException) { DeleteCookie(); throw; }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        RequireAjax();
        await _auth.LogoutAsync(Request.Cookies[CookieName]);
        DeleteCookie();
        return NoContent();
    }

    // Defensa extra contra CSRF (la cookie ya es SameSite=Strict): solo fetch propio envía esta cabecera.
    private void RequireAjax()
    {
        if (!Request.Headers.ContainsKey("X-Requested-With")) throw AppException.BadRequest("Solicitud no valida");
    }

    private CookieOptions Options(DateTime? expires) => new()
    {
        HttpOnly = true,
        Secure = Request.IsHttps || !_env.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = "/api/auth",
        IsEssential = true,
        Expires = expires.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(expires.Value, DateTimeKind.Utc)) : null
    };

    private void SetCookie(string token, DateTime expiresUtc) => Response.Cookies.Append(CookieName, token, Options(expiresUtc));
    private void DeleteCookie() => Response.Cookies.Delete(CookieName, Options(null));
}
