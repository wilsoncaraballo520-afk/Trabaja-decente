using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;
using TrabajaDecente.Api.Models;
using TrabajaDecente.Api.Services;

namespace TrabajaDecente.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Administrador)]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _svc;
    public AdminController(IAdminService svc) => _svc = svc;

    [HttpGet("resumen")] public async Task<ResumenAdminRow> Resumen() => await _svc.ResumenAsync();

    [HttpGet("empresas")]
    public async Task<List<EmpresaAdminRow>> Empresas([FromQuery] string? aprobacion) => await _svc.EmpresasAsync(aprobacion);

    [HttpPost("empresas/{id:int}/aprobar")]
    public async Task<MensajeResponse> Aprobar(int id)
    {
        await _svc.AprobarAsync(id, User.GetUserId());
        return new MensajeResponse("Empresa aprobada");
    }

    [HttpPost("empresas/{id:int}/rechazar")]
    public async Task<MensajeResponse> Rechazar(int id)
    {
        await _svc.RechazarAsync(id, User.GetUserId());
        return new MensajeResponse("Empresa rechazada");
    }

    [HttpPatch("empresas/{id:int}/estado")]
    public async Task<MensajeResponse> EstadoEmpresa(int id, ActivoRequest req)
    {
        await _svc.CambiarEstadoEmpresaAsync(id, req.Activo);
        return new MensajeResponse(req.Activo ? "Empresa activada" : "Empresa desactivada");
    }

    [HttpGet("categorias")] public async Task<List<CategoriaRow>> Categorias() => await _svc.CategoriasAsync();

    [HttpPost("categorias")]
    public async Task<ActionResult<MensajeResponse>> CrearCategoria(CategoriaRequest req) =>
        StatusCode(201, new MensajeResponse("Categoria creada", await _svc.CrearCategoriaAsync(req.NombreCategoria)));

    [HttpPatch("categorias/{id:int}/estado")]
    public async Task<MensajeResponse> EstadoCategoria(int id, ActivoRequest req)
    {
        await _svc.CambiarEstadoCategoriaAsync(id, req.Activo);
        return new MensajeResponse(req.Activo ? "Categoria activada" : "Categoria desactivada");
    }
}
