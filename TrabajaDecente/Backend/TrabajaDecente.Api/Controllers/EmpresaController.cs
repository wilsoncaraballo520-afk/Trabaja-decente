using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;
using TrabajaDecente.Api.Models;
using TrabajaDecente.Api.Services;

namespace TrabajaDecente.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Empresa)]
[Route("api/empresa")]
public class EmpresaController : ControllerBase
{
    private readonly IEmpresaService _svc;
    public EmpresaController(IEmpresaService svc) => _svc = svc;
    private int Id => User.GetUserId();

    [HttpGet("resumen")] public async Task<ResumenEmpresaRow> Resumen() => await _svc.ResumenAsync(Id);
    [HttpGet("ofertas")] public async Task<List<OfertaEmpresaRow>> Ofertas() => await _svc.OfertasAsync(Id);

    [HttpPost("ofertas")]
    public async Task<ActionResult<MensajeResponse>> Publicar(OfertaRequest req) =>
        StatusCode(201, new MensajeResponse("Oferta publicada", await _svc.PublicarAsync(Id, req)));

    [HttpPut("ofertas/{idOferta:int}")]
    public async Task<MensajeResponse> Editar(int idOferta, OfertaRequest req)
    {
        await _svc.EditarAsync(idOferta, Id, req);
        return new MensajeResponse("Oferta actualizada");
    }

    [HttpPost("ofertas/{idOferta:int}/cerrar")]
    public async Task<MensajeResponse> Cerrar(int idOferta)
    {
        await _svc.CerrarAsync(idOferta, Id);
        return new MensajeResponse("Oferta cerrada");
    }

    [HttpGet("postulaciones")]
    public async Task<List<PostulacionEmpresaRow>> Postulaciones([FromQuery] int? idOferta) =>
        await _svc.PostulacionesAsync(Id, idOferta);

    [HttpPatch("postulaciones/{idPostulacion:int}/estado")]
    public async Task<MensajeResponse> CambiarEstado(int idPostulacion, EstadoPostulacionRequest req)
    {
        await _svc.ActualizarEstadoAsync(Id, idPostulacion, req.Estado);
        return new MensajeResponse("Estado actualizado");
    }

    [HttpGet("postulaciones/{idPostulacion:int}/hoja-de-vida")]
    public async Task<IActionResult> HojaVida(int idPostulacion)
    {
        var f = await _svc.DescargarHojaVidaAsync(Id, idPostulacion);
        return File(f.ArchivoPDF, "application/pdf", f.NombreArchivo);
    }
}
