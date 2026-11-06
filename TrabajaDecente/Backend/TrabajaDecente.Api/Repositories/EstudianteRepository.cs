using TrabajaDecente.Api.Data;
using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Models;

namespace TrabajaDecente.Api.Repositories;

public interface IEstudianteRepository
{
    Task<ResumenEstudianteRow?> ResumenAsync(int id);
    Task<PerfilEstudianteRow?> PerfilAsync(int id);
    Task ActualizarPerfilAsync(int id, ActualizarPerfilRequest r);
    Task<List<PostulacionEstudianteRow>> PostulacionesAsync(int id);
    Task<long> PostularAsync(int idEstudiante, int idOferta);
    Task<List<FavoritaRow>> FavoritasAsync(int id);
    Task GuardarFavoritaAsync(int idEstudiante, int idOferta);
    Task EliminarFavoritaAsync(int idEstudiante, int idOferta);
    Task<HojaVidaMetaRow?> HojaVidaMetaAsync(int id);
    Task<HojaVidaMetaRow?> GuardarHojaVidaAsync(int id, string nombre, long tamano, byte[] pdf);
    Task<HojaVidaArchivoRow?> DescargarHojaVidaAsync(int id);
    Task EliminarHojaVidaAsync(int id);
}

public class EstudianteRepository : IEstudianteRepository
{
    private readonly IDbExecutor _db;
    public EstudianteRepository(IDbExecutor db) => _db = db;

    public Task<ResumenEstudianteRow?> ResumenAsync(int id) => _db.FirstOrDefaultAsync<ResumenEstudianteRow>("sp_ResumenEstudiante", id);
    public Task<PerfilEstudianteRow?> PerfilAsync(int id) => _db.FirstOrDefaultAsync<PerfilEstudianteRow>("sp_VerPerfilEstudiante", id);

    public Task ActualizarPerfilAsync(int id, ActualizarPerfilRequest r) =>
        _db.ExecuteAsync("sp_ActualizarPerfilEstudiante", id, r.Nombres, r.PrimerApellido, r.SegundoApellido, r.Telefono);

    public Task<List<PostulacionEstudianteRow>> PostulacionesAsync(int id) =>
        _db.QueryAsync<PostulacionEstudianteRow>("sp_VerPostulacionesEstudiante", id);

    public async Task<long> PostularAsync(int idEstudiante, int idOferta) =>
        (await _db.FirstOrDefaultAsync<NuevoIdRow>("sp_PostularOferta", idEstudiante, idOferta))?.Valor ?? 0;

    public Task<List<FavoritaRow>> FavoritasAsync(int id) => _db.QueryAsync<FavoritaRow>("sp_VerFavoritas", id);
    public Task GuardarFavoritaAsync(int e, int o) => _db.ExecuteAsync("sp_GuardarFavorita", e, o);
    public Task EliminarFavoritaAsync(int e, int o) => _db.ExecuteAsync("sp_EliminarFavorita", e, o);

    public Task<HojaVidaMetaRow?> HojaVidaMetaAsync(int id) => _db.FirstOrDefaultAsync<HojaVidaMetaRow>("sp_ObtenerHojaVidaEstudiante", id);

    public Task<HojaVidaMetaRow?> GuardarHojaVidaAsync(int id, string nombre, long tamano, byte[] pdf) =>
        _db.FirstOrDefaultAsync<HojaVidaMetaRow>("sp_GuardarHojaVida", id, nombre, "application/pdf", tamano, pdf);

    public Task<HojaVidaArchivoRow?> DescargarHojaVidaAsync(int id) =>
        _db.FirstOrDefaultAsync<HojaVidaArchivoRow>("sp_DescargarMiHojaVida", id);

    public Task EliminarHojaVidaAsync(int id) => _db.ExecuteAsync("sp_EliminarHojaVida", id);
}
