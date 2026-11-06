using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;
using TrabajaDecente.Api.Models;
using TrabajaDecente.Api.Repositories;

namespace TrabajaDecente.Api.Services;

public interface IEmpresaService
{
    Task<ResumenEmpresaRow> ResumenAsync(int idEmpresa);
    Task<List<OfertaEmpresaRow>> OfertasAsync(int idEmpresa);
    Task<long> PublicarAsync(int idEmpresa, OfertaRequest r);
    Task EditarAsync(int idOferta, int idEmpresa, OfertaRequest r);
    Task CerrarAsync(int idOferta, int idEmpresa);
    Task<List<PostulacionEmpresaRow>> PostulacionesAsync(int idEmpresa, int? idOferta);
    Task ActualizarEstadoAsync(int idEmpresa, int idPostulacion, string estado);
    Task<HojaVidaArchivoRow> DescargarHojaVidaAsync(int idEmpresa, int idPostulacion);
}

public class EmpresaService : IEmpresaService
{
    private readonly IEmpresaRepository _repo;
    private readonly IOfertaRepository _ofertas;
    public EmpresaService(IEmpresaRepository repo, IOfertaRepository ofertas) { _repo = repo; _ofertas = ofertas; }

    public async Task<ResumenEmpresaRow> ResumenAsync(int id) => await _repo.ResumenAsync(id) ?? new();
    public Task<List<OfertaEmpresaRow>> OfertasAsync(int id) => _ofertas.DeEmpresaAsync(id);

    // La empresa SIEMPRE viene del token: nunca se acepta un IdEmpresa del cliente.
    public Task<long> PublicarAsync(int idEmpresa, OfertaRequest r) => _ofertas.PublicarAsync(idEmpresa, Limpiar(r));
    public Task EditarAsync(int idOferta, int idEmpresa, OfertaRequest r) => _ofertas.EditarAsync(idOferta, idEmpresa, Limpiar(r));
    public Task CerrarAsync(int idOferta, int idEmpresa) => _ofertas.CerrarAsync(idOferta, idEmpresa);

    public Task<List<PostulacionEmpresaRow>> PostulacionesAsync(int idEmpresa, int? idOferta) =>
        _repo.PostulacionesAsync(idEmpresa, idOferta);

    public Task ActualizarEstadoAsync(int idEmpresa, int idPostulacion, string estado) =>
        _repo.ActualizarEstadoPostulacionAsync(idEmpresa, idPostulacion, estado);

    public async Task<HojaVidaArchivoRow> DescargarHojaVidaAsync(int idEmpresa, int idPostulacion) =>
        await _repo.DescargarHojaVidaAsync(idEmpresa, idPostulacion)
        ?? throw AppException.NotFound("El candidato no cargo hoja de vida");

    private static OfertaRequest Limpiar(OfertaRequest r)
    {
        r.Titulo = r.Titulo.Trim();
        r.Descripcion = r.Descripcion.Trim();
        r.Horario = r.Horario.Trim();
        return r;
    }
}

public interface IAdminService
{
    Task<ResumenAdminRow> ResumenAsync();
    Task<List<EmpresaAdminRow>> EmpresasAsync(string? aprobacion);
    Task AprobarAsync(int idEmpresa, int idAdmin);
    Task RechazarAsync(int idEmpresa, int idAdmin);
    Task CambiarEstadoEmpresaAsync(int idEmpresa, bool activo);
    Task<List<CategoriaRow>> CategoriasAsync();
    Task<long> CrearCategoriaAsync(string nombre);
    Task CambiarEstadoCategoriaAsync(int id, bool activo);
}

public class AdminService : IAdminService
{
    private static readonly string[] Estados = { "Pendiente", "Aprobada", "Rechazada" };
    private readonly IAdminRepository _repo;
    public AdminService(IAdminRepository repo) => _repo = repo;

    public async Task<ResumenAdminRow> ResumenAsync() => await _repo.ResumenAsync() ?? new();

    public async Task<List<EmpresaAdminRow>> EmpresasAsync(string? aprobacion)
    {
        var todas = await _repo.EmpresasAsync();
        if (string.IsNullOrWhiteSpace(aprobacion)) return todas;
        if (!Estados.Contains(aprobacion)) throw AppException.BadRequest("Estado de aprobacion no valido");
        return todas.Where(e => e.EstadoAprobacion == aprobacion).ToList();
    }

    public Task AprobarAsync(int e, int a) => _repo.AprobarAsync(e, a);
    public Task RechazarAsync(int e, int a) => _repo.RechazarAsync(e, a);
    public Task CambiarEstadoEmpresaAsync(int e, bool activo) => _repo.CambiarEstadoEmpresaAsync(e, activo);
    public Task<List<CategoriaRow>> CategoriasAsync() => _repo.CategoriasAsync();
    public Task<long> CrearCategoriaAsync(string nombre) => _repo.CrearCategoriaAsync(nombre.Trim());
    public Task CambiarEstadoCategoriaAsync(int id, bool activo) => _repo.CambiarEstadoCategoriaAsync(id, activo);
}
