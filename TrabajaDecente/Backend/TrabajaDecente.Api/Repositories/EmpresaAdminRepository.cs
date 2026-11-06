using TrabajaDecente.Api.Data;
using TrabajaDecente.Api.Models;

namespace TrabajaDecente.Api.Repositories;

public interface IEmpresaRepository
{
    Task<ResumenEmpresaRow?> ResumenAsync(int idEmpresa);
    Task<List<PostulacionEmpresaRow>> PostulacionesAsync(int idEmpresa, int? idOferta);
    Task ActualizarEstadoPostulacionAsync(int idEmpresa, int idPostulacion, string estado);
    Task<HojaVidaArchivoRow?> DescargarHojaVidaAsync(int idEmpresa, int idPostulacion);
}

public class EmpresaRepository : IEmpresaRepository
{
    private readonly IDbExecutor _db;
    public EmpresaRepository(IDbExecutor db) => _db = db;

    public Task<ResumenEmpresaRow?> ResumenAsync(int id) => _db.FirstOrDefaultAsync<ResumenEmpresaRow>("sp_ResumenEmpresa", id);

    public Task<List<PostulacionEmpresaRow>> PostulacionesAsync(int idEmpresa, int? idOferta) =>
        _db.QueryAsync<PostulacionEmpresaRow>("sp_VerPostulacionesEmpresa", idEmpresa, idOferta);

    public Task ActualizarEstadoPostulacionAsync(int idEmpresa, int idPostulacion, string estado) =>
        _db.ExecuteAsync("sp_ActualizarEstadoPostulacion", idEmpresa, idPostulacion, estado);

    public Task<HojaVidaArchivoRow?> DescargarHojaVidaAsync(int idEmpresa, int idPostulacion) =>
        _db.FirstOrDefaultAsync<HojaVidaArchivoRow>("sp_DescargarHojaVidaPorPostulacion", idEmpresa, idPostulacion);
}

public interface IAdminRepository
{
    Task<ResumenAdminRow?> ResumenAsync();
    Task<List<EmpresaAdminRow>> EmpresasAsync();
    Task AprobarAsync(int idEmpresa, int idAdmin);
    Task RechazarAsync(int idEmpresa, int idAdmin);
    Task CambiarEstadoEmpresaAsync(int idEmpresa, bool activo);
    Task<List<CategoriaRow>> CategoriasAsync();
    Task<long> CrearCategoriaAsync(string nombre);
    Task CambiarEstadoCategoriaAsync(int id, bool activo);
}

public class AdminRepository : IAdminRepository
{
    private readonly IDbExecutor _db;
    public AdminRepository(IDbExecutor db) => _db = db;

    public Task<ResumenAdminRow?> ResumenAsync() => _db.FirstOrDefaultAsync<ResumenAdminRow>("sp_ResumenAdministrador");
    public Task<List<EmpresaAdminRow>> EmpresasAsync() => _db.QueryAsync<EmpresaAdminRow>("sp_ListarEmpresas");
    public Task AprobarAsync(int e, int a) => _db.ExecuteAsync("sp_AprobarEmpresa", e, a);
    public Task RechazarAsync(int e, int a) => _db.ExecuteAsync("sp_RechazarEmpresa", e, a);
    public Task CambiarEstadoEmpresaAsync(int e, bool activo) => _db.ExecuteAsync("sp_CambiarEstadoEmpresa", e, activo ? 1 : 0);
    public Task<List<CategoriaRow>> CategoriasAsync() => _db.QueryAsync<CategoriaRow>("sp_ListarCategoriasAdmin");

    public async Task<long> CrearCategoriaAsync(string nombre) =>
        (await _db.FirstOrDefaultAsync<NuevoIdRow>("sp_CrearCategoria", nombre))?.Valor ?? 0;

    public Task CambiarEstadoCategoriaAsync(int id, bool activo) =>
        _db.ExecuteAsync("sp_CambiarEstadoCategoria", id, activo ? 1 : 0);
}
