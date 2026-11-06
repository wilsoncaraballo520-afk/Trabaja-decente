using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;
using TrabajaDecente.Api.Models;
using TrabajaDecente.Api.Repositories;

namespace TrabajaDecente.Api.Services;

public interface IEstudianteService
{
    Task<ResumenEstudianteRow> ResumenAsync(int id);
    Task<PerfilEstudianteRow> PerfilAsync(int id);
    Task ActualizarPerfilAsync(int id, ActualizarPerfilRequest r);
    Task<List<PostulacionEstudianteRow>> PostulacionesAsync(int id);
    Task<long> PostularAsync(int idEstudiante, int idOferta);
    Task<List<FavoritaRow>> FavoritasAsync(int id);
    Task GuardarFavoritaAsync(int idEstudiante, int idOferta);
    Task EliminarFavoritaAsync(int idEstudiante, int idOferta);
    Task<HojaVidaMetaRow?> HojaVidaMetaAsync(int id);
    Task<HojaVidaMetaRow> SubirHojaVidaAsync(int id, IFormFile? archivo);
    Task<HojaVidaArchivoRow> DescargarHojaVidaAsync(int id);
    Task EliminarHojaVidaAsync(int id);
}

public class EstudianteService : IEstudianteService
{
    private readonly IEstudianteRepository _repo;
    public EstudianteService(IEstudianteRepository repo) => _repo = repo;

    public async Task<ResumenEstudianteRow> ResumenAsync(int id) => await _repo.ResumenAsync(id) ?? new();

    public async Task<PerfilEstudianteRow> PerfilAsync(int id) =>
        await _repo.PerfilAsync(id) ?? throw AppException.NotFound("Perfil no encontrado");

    public Task ActualizarPerfilAsync(int id, ActualizarPerfilRequest r)
    {
        r.Nombres = r.Nombres.Trim();
        r.PrimerApellido = r.PrimerApellido.Trim();
        r.SegundoApellido = string.IsNullOrWhiteSpace(r.SegundoApellido) ? null : r.SegundoApellido.Trim();
        r.Telefono = r.Telefono.Trim();
        return _repo.ActualizarPerfilAsync(id, r);
    }

    public Task<List<PostulacionEstudianteRow>> PostulacionesAsync(int id) => _repo.PostulacionesAsync(id);
    public Task<long> PostularAsync(int idEstudiante, int idOferta) => _repo.PostularAsync(idEstudiante, idOferta);
    public Task<List<FavoritaRow>> FavoritasAsync(int id) => _repo.FavoritasAsync(id);
    public Task GuardarFavoritaAsync(int e, int o) => _repo.GuardarFavoritaAsync(e, o);
    public Task EliminarFavoritaAsync(int e, int o) => _repo.EliminarFavoritaAsync(e, o);
    public Task<HojaVidaMetaRow?> HojaVidaMetaAsync(int id) => _repo.HojaVidaMetaAsync(id);
    public Task EliminarHojaVidaAsync(int id) => _repo.EliminarHojaVidaAsync(id);

    public async Task<HojaVidaMetaRow> SubirHojaVidaAsync(int id, IFormFile? archivo)
    {
        if (archivo is null || archivo.Length == 0) throw AppException.BadRequest("Selecciona un archivo PDF");
        if (archivo.Length > PdfValidator.MaxBytes) throw AppException.BadRequest("El archivo supera 5 MB");

        using var ms = new MemoryStream((int)archivo.Length);
        await archivo.CopyToAsync(ms);
        var bytes = ms.ToArray();
        PdfValidator.Validate(bytes);

        var meta = await _repo.GuardarHojaVidaAsync(id, PdfValidator.SafeName(archivo.FileName), bytes.LongLength, bytes);
        return meta ?? throw AppException.BadRequest("No se pudo guardar la hoja de vida");
    }

    public async Task<HojaVidaArchivoRow> DescargarHojaVidaAsync(int id) =>
        await _repo.DescargarHojaVidaAsync(id) ?? throw AppException.NotFound("Aun no has cargado tu hoja de vida");
}
