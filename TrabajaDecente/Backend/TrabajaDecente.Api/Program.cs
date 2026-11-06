using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using TrabajaDecente.Api.Data;
using TrabajaDecente.Api.Middlewares;
using TrabajaDecente.Api.Repositories;
using TrabajaDecente.Api.Services;
using TrabajaDecente.Api.Validators;

// Utilidad para crear el primer administrador:  dotnet run -- --hash "TuContrasenaSegura1"
if (args.Length == 2 && args[0] == "--hash")
{
    Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(args[1], 12));
    return;
}

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuración (los secretos NO van en el repositorio) ----------
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
    throw new InvalidOperationException("Configura Jwt:Key (minimo 32 caracteres) con 'dotnet user-secrets' o la variable de entorno Jwt__Key.");
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Configura ConnectionStrings:Default con 'dotnet user-secrets' o la variable de entorno ConnectionStrings__Default.");

builder.Services.AddSingleton(jwt);
builder.WebHost.ConfigureKestrel(o => { o.AddServerHeader = false; o.Limits.MaxRequestBodySize = 6 * 1024 * 1024; });

// ---------- Base de datos: EF Core + Pomelo (MySqlConnector) ----------
builder.Services.AddDbContext<AppDbContext>(o => o.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));
builder.Services.AddScoped<IDbExecutor, DbExecutor>();

// ---------- Repositorios y servicios ----------
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IOfertaRepository, OfertaRepository>();
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOfertaService, OfertaService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// ---------- Validación ----------
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();
builder.Services.AddControllers(o => o.Filters.Add<ValidationFilter>())
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = _ =>
        new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { error = "Solicitud no valida" }));

// ---------- JWT (access token de 15 min; el refresh token va en cookie HttpOnly) ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.MapInboundClaims = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt.Issuer,
        ValidateAudience = true, ValidAudience = jwt.Audience,
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        NameClaimType = "name", RoleClaimType = "role"
    };
});
builder.Services.AddAuthorization();

// ---------- Límite de intentos en login y registro ----------
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
if (!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseHttpsRedirection(); }

// El frontend se sirve desde el mismo origen (cookies SameSite=Strict, sin CORS).
var frontendPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, builder.Configuration["Frontend:Path"] ?? "../../Frontend"));
if (Directory.Exists(frontendPath))
{
    var files = new PhysicalFileProvider(frontendPath);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = files });
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Map("/api/{**resto}", () => Results.NotFound(new { error = "Recurso no encontrado" }));

app.Run();
