using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;
using TrabajaDecente.Api.Repositories;

namespace TrabajaDecente.Api.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest req);
    Task<AuthResult> RefreshAsync(string? refreshToken);
    Task LogoutAsync(string? refreshToken);
    Task<MensajeResponse> RegistrarEstudianteAsync(RegistroEstudianteRequest req);
    Task<MensajeResponse> RegistrarEmpresaAsync(RegistroEmpresaRequest req);
}

public class AuthService : IAuthService
{
    private const int GraciaRotacionSegundos = 10; // tolera dos pestañas renovando a la vez
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("contrasena-ficticia-para-igualar-tiempos", 12);

    private readonly IAuthRepository _repo;
    private readonly ITokenService _tokens;
    private readonly JwtOptions _jwt;

    public AuthService(IAuthRepository repo, ITokenService tokens, JwtOptions jwt)
    {
        _repo = repo; _tokens = tokens; _jwt = jwt;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest req)
    {
        var cred = await _repo.ObtenerCredencialesAsync(req.Rol, req.Correo.Trim().ToLowerInvariant());

        // Siempre se verifica un hash (aunque el usuario no exista) para no revelar qué correos están registrados.
        var ok = BCrypt.Net.BCrypt.Verify(req.Contrasena, cred?.ContrasenaHash ?? DummyHash);
        if (cred is null || !ok || !cred.Estado)
            throw AppException.Unauthorized("Correo o contrasena incorrectos");

        return await EmitirAsync(req.Rol, cred.IdUsuario, cred.Nombre, cred.EstadoAprobacion);
    }

    public async Task<AuthResult> RefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) throw AppException.Unauthorized("Sesion no iniciada");

        var hash = _tokens.Hash(refreshToken);
        var row = await _repo.ObtenerRefreshAsync(hash) ?? throw AppException.Unauthorized("Sesion no valida");

        if (row.Revocado)
        {
            // Un token ya usado que reaparece fuera de la ventana de gracia sugiere robo: se cierran todas las sesiones.
            if ((row.SegundosDesdeRevocacion ?? long.MaxValue) > GraciaRotacionSegundos)
                await _repo.RevocarTodosAsync(row.Rol, row.IdUsuario);
            throw AppException.Unauthorized("Sesion no valida");
        }

        if (DateTime.SpecifyKind(row.FechaExpiracion, DateTimeKind.Utc) <= DateTime.UtcNow)
            throw AppException.Unauthorized("La sesion expiro");

        var sesion = await _repo.ObtenerSesionAsync(row.Rol, row.IdUsuario);
        if (sesion is null || !sesion.Estado)
        {
            await _repo.RevocarTodosAsync(row.Rol, row.IdUsuario);
            throw AppException.Unauthorized("Cuenta inactiva");
        }

        await _repo.RevocarRefreshAsync(hash); // rotación: cada token sirve una sola vez
        return await EmitirAsync(row.Rol, sesion.IdUsuario, sesion.Nombre, sesion.EstadoAprobacion);
    }

    public async Task LogoutAsync(string? refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
            await _repo.RevocarRefreshAsync(_tokens.Hash(refreshToken));
    }

    public async Task<MensajeResponse> RegistrarEstudianteAsync(RegistroEstudianteRequest r)
    {
        r.Nombres = r.Nombres.Trim();
        r.PrimerApellido = r.PrimerApellido.Trim();
        r.SegundoApellido = string.IsNullOrWhiteSpace(r.SegundoApellido) ? null : r.SegundoApellido.Trim();
        r.NumeroDocumento = r.NumeroDocumento.Trim();
        r.CorreoElectronico = r.CorreoElectronico.Trim().ToLowerInvariant();
        r.Telefono = r.Telefono.Trim();

        var id = await _repo.RegistrarEstudianteAsync(r, BCrypt.Net.BCrypt.HashPassword(r.Contrasena, 12));
        return new MensajeResponse("Cuenta creada. Ya puedes iniciar sesion.", id);
    }

    public async Task<MensajeResponse> RegistrarEmpresaAsync(RegistroEmpresaRequest r)
    {
        r.NombreEmpresa = r.NombreEmpresa.Trim();
        r.Nit = r.Nit.Trim();
        r.CorreoElectronico = r.CorreoElectronico.Trim().ToLowerInvariant();
        r.Telefono = r.Telefono.Trim();
        r.Direccion = string.IsNullOrWhiteSpace(r.Direccion) ? null : r.Direccion.Trim();

        var id = await _repo.RegistrarEmpresaAsync(r, BCrypt.Net.BCrypt.HashPassword(r.Contrasena, 12));
        return new MensajeResponse("Empresa registrada. Un administrador debe aprobarla antes de publicar ofertas.", id);
    }

    private async Task<AuthResult> EmitirAsync(string rol, int id, string nombre, string? estadoAprobacion)
    {
        var access = _tokens.CreateAccessToken(id, rol, nombre);
        var (raw, hash, expires) = _tokens.CreateRefreshToken();
        await _repo.GuardarRefreshAsync(rol, id, hash, expires);
        var resp = new AuthResponse(access, _jwt.AccessMinutes * 60, rol, nombre, estadoAprobacion);
        return new AuthResult(resp, raw, expires);
    }
}
