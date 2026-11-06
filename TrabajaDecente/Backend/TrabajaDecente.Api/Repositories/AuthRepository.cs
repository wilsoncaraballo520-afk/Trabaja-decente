using TrabajaDecente.Api.Data;
using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;
using TrabajaDecente.Api.Models;

namespace TrabajaDecente.Api.Repositories;

public interface IAuthRepository
{
    Task<CredencialRow?> ObtenerCredencialesAsync(string rol, string correo);
    Task<SesionRow?> ObtenerSesionAsync(string rol, int id);
    Task<long> RegistrarEstudianteAsync(RegistroEstudianteRequest r, string hash);
    Task<long> RegistrarEmpresaAsync(RegistroEmpresaRequest r, string hash);
    Task GuardarRefreshAsync(string rol, int idUsuario, string tokenHash, DateTime expiraUtc);
    Task<RefreshTokenRow?> ObtenerRefreshAsync(string tokenHash);
    Task RevocarRefreshAsync(string tokenHash);
    Task RevocarTodosAsync(string rol, int idUsuario);
}

public class AuthRepository : IAuthRepository
{
    private readonly IDbExecutor _db;
    public AuthRepository(IDbExecutor db) => _db = db;

    public Task<CredencialRow?> ObtenerCredencialesAsync(string rol, string correo)
    {
        var sp = rol switch
        {
            Roles.Estudiante => "sp_ObtenerCredencialesEstudiante",
            Roles.Empresa => "sp_ObtenerCredencialesEmpresa",
            Roles.Administrador => "sp_ObtenerCredencialesAdministrador",
            _ => throw AppException.BadRequest("Rol no valido")
        };
        return _db.FirstOrDefaultAsync<CredencialRow>(sp, correo);
    }

    public Task<SesionRow?> ObtenerSesionAsync(string rol, int id)
    {
        var sp = rol switch
        {
            Roles.Estudiante => "sp_ObtenerEstudiantePorId",
            Roles.Empresa => "sp_ObtenerEmpresaPorId",
            Roles.Administrador => "sp_ObtenerAdministradorPorId",
            _ => throw AppException.BadRequest("Rol no valido")
        };
        return _db.FirstOrDefaultAsync<SesionRow>(sp, id);
    }

    public async Task<long> RegistrarEstudianteAsync(RegistroEstudianteRequest r, string hash)
    {
        var row = await _db.FirstOrDefaultAsync<NuevoIdRow>("sp_RegistrarEstudiante",
            r.TipoDocumento, r.NumeroDocumento, r.Nombres, r.PrimerApellido, r.SegundoApellido,
            r.FechaNacimiento.Date, r.CorreoElectronico, r.Telefono, hash);
        return row?.Valor ?? 0;
    }

    public async Task<long> RegistrarEmpresaAsync(RegistroEmpresaRequest r, string hash)
    {
        var row = await _db.FirstOrDefaultAsync<NuevoIdRow>("sp_RegistrarEmpresa",
            r.NombreEmpresa, r.Nit, r.CorreoElectronico, r.Telefono, r.Direccion, hash);
        return row?.Valor ?? 0;
    }

    public Task GuardarRefreshAsync(string rol, int idUsuario, string tokenHash, DateTime expiraUtc) =>
        _db.ExecuteAsync("sp_GuardarRefreshToken", rol, idUsuario, tokenHash, expiraUtc);

    public Task<RefreshTokenRow?> ObtenerRefreshAsync(string tokenHash) =>
        _db.FirstOrDefaultAsync<RefreshTokenRow>("sp_ObtenerRefreshToken", tokenHash);

    public Task RevocarRefreshAsync(string tokenHash) =>
        _db.ExecuteAsync("sp_RevocarRefreshToken", tokenHash);

    public Task RevocarTodosAsync(string rol, int idUsuario) =>
        _db.ExecuteAsync("sp_RevocarRefreshTokensUsuario", rol, idUsuario);
}
