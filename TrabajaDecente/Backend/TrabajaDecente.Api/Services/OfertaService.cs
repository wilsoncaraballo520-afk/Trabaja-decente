using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;
using TrabajaDecente.Api.Models;
using TrabajaDecente.Api.Repositories;

namespace TrabajaDecente.Api.Services;

public interface IOfertaService
{
    Task<PagedResult<OfertaRow>> ListarAsync(OfertaFiltro f);
    Task<OfertaRow> DetalleAsync(int id);
    Task<List<CategoriaRow>> CategoriasAsync(bool conConteo);
    Task<List<CiudadRow>> CiudadesAsync();
}

public class OfertaService : IOfertaService
{
    private readonly IOfertaRepository _repo;
    public OfertaService(IOfertaRepository repo) => _repo = repo;

    public async Task<PagedResult<OfertaRow>> ListarAsync(OfertaFiltro f)
    {
        var q = string.IsNullOrWhiteSpace(f.Q) ? null : EscapeLike(f.Q.Trim());
        var rows = await _repo.FiltrarAsync(q, f.Categoria, f.Ciudad,
            Vacio(f.Modalidad), Vacio(f.TipoContrato), f.SalarioMin, f.SalarioMax);

        IEnumerable<OfertaRow> ordered = f.Orden switch
        {
            "salario_desc" => rows.OrderByDescending(o => o.Salario ?? 0),
            "salario_asc" => rows.OrderBy(o => o.Salario ?? decimal.MaxValue),
            "cierre" => rows.OrderBy(o => o.FechaCierre),
            _ => rows.OrderByDescending(o => o.FechaPublicacion)
        };

        var total = rows.Count;
        var pages = Math.Max(1, (int)Math.Ceiling(total / (double)f.Tamano));
        var pagina = Math.Min(Math.Max(1, f.Pagina), pages);
        var items = ordered.Skip((pagina - 1) * f.Tamano).Take(f.Tamano).ToList();
        return new PagedResult<OfertaRow>(items, total, pagina, f.Tamano, pages);
    }

    public async Task<OfertaRow> DetalleAsync(int id) =>
        await _repo.DetalleAsync(id) ?? throw AppException.NotFound("La oferta no esta disponible");

    public Task<List<CategoriaRow>> CategoriasAsync(bool conConteo) =>
        conConteo ? _repo.CategoriasResumenAsync() : _repo.CategoriasAsync();

    public Task<List<CiudadRow>> CiudadesAsync() => _repo.CiudadesAsync();

    private static string? Vacio(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    // El procedimiento arma LIKE '%' + texto + '%'; se escapan los comodines para buscar texto literal.
    private static string EscapeLike(string s) => s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
