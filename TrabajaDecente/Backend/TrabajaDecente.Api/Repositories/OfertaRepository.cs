using TrabajaDecente.Api.Data;
using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Models;

namespace TrabajaDecente.Api.Repositories;

public interface IOfertaRepository
{
    Task<List<OfertaRow>> FiltrarAsync(string? q, int? categoria, int? ciudad, string? modalidad,
        string? tipo, decimal? salMin, decimal? salMax);
    Task<OfertaRow?> DetalleAsync(int id);
    Task<List<CategoriaRow>> CategoriasAsync();
    Task<List<CategoriaRow>> CategoriasResumenAsync();
    Task<List<CiudadRow>> CiudadesAsync();
    Task<long> PublicarAsync(int idEmpresa, OfertaRequest r);
    Task EditarAsync(int idOferta, int idEmpresa, OfertaRequest r);
    Task CerrarAsync(int idOferta, int idEmpresa);
    Task<List<OfertaEmpresaRow>> DeEmpresaAsync(int idEmpresa);
}

public class OfertaRepository : IOfertaRepository
{
    private readonly IDbExecutor _db;
    public OfertaRepository(IDbExecutor db) => _db = db;

    public Task<List<OfertaRow>> FiltrarAsync(string? q, int? categoria, int? ciudad, string? modalidad,
        string? tipo, decimal? salMin, decimal? salMax) =>
        _db.QueryAsync<OfertaRow>("sp_FiltrarOfertas", q, categoria, ciudad, modalidad, tipo, salMin, salMax);

    public Task<OfertaRow?> DetalleAsync(int id) => _db.FirstOrDefaultAsync<OfertaRow>("sp_VerDetalleOferta", id);
    public Task<List<CategoriaRow>> CategoriasAsync() => _db.QueryAsync<CategoriaRow>("sp_ListarCategorias");
    public Task<List<CategoriaRow>> CategoriasResumenAsync() => _db.QueryAsync<CategoriaRow>("sp_ContarOfertasPorCategoria");
    public Task<List<CiudadRow>> CiudadesAsync() => _db.QueryAsync<CiudadRow>("sp_ListarCiudades");

    public async Task<long> PublicarAsync(int idEmpresa, OfertaRequest r)
    {
        var row = await _db.FirstOrDefaultAsync<NuevoIdRow>("sp_PublicarOferta",
            idEmpresa, r.IdCategoria, r.IdCiudad, r.Titulo, r.Descripcion, r.Salario, r.TipoContrato,
            r.Modalidad, r.Horario, r.Vacantes, r.ExperienciaMinimaMeses, r.FechaCierre);
        return row?.Valor ?? 0;
    }

    public Task EditarAsync(int idOferta, int idEmpresa, OfertaRequest r) =>
        _db.ExecuteAsync("sp_EditarOferta", idOferta, idEmpresa, r.IdCategoria, r.IdCiudad, r.Titulo,
            r.Descripcion, r.Salario, r.TipoContrato, r.Modalidad, r.Horario, r.Vacantes,
            r.ExperienciaMinimaMeses, r.FechaCierre);

    public Task CerrarAsync(int idOferta, int idEmpresa) => _db.ExecuteAsync("sp_CerrarOferta", idOferta, idEmpresa);
    public Task<List<OfertaEmpresaRow>> DeEmpresaAsync(int idEmpresa) =>
        _db.QueryAsync<OfertaEmpresaRow>("sp_VerOfertasEmpresa", idEmpresa);
}
