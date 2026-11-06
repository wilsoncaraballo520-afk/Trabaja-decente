using Microsoft.AspNetCore.Mvc;
using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Models;
using TrabajaDecente.Api.Services;

namespace TrabajaDecente.Api.Controllers;

/// <summary>Consulta pública: ofertas abiertas de empresas aprobadas, categorías y municipios.</summary>
[ApiController]
[Route("api")]
public class PublicController : ControllerBase
{
    private readonly IOfertaService _svc;
    public PublicController(IOfertaService svc) => _svc = svc;

    [HttpGet("ofertas")]
    public async Task<PagedResult<OfertaRow>> Ofertas([FromQuery] OfertaFiltro filtro) => await _svc.ListarAsync(filtro);

    [HttpGet("ofertas/{id:int}")]
    public async Task<OfertaRow> Oferta(int id) => await _svc.DetalleAsync(id);

    [HttpGet("categorias")]
    public async Task<List<CategoriaRow>> Categorias([FromQuery] bool conteo = false) => await _svc.CategoriasAsync(conteo);

    [HttpGet("ciudades")]
    public async Task<List<CiudadRow>> Ciudades() => await _svc.CiudadesAsync();
}
