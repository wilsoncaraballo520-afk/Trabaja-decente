using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;
using TrabajaDecente.Api.Models;
using TrabajaDecente.Api.Services;

namespace TrabajaDecente.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Estudiante)]
[Route("api/estudiante")]
public class EstudianteController : ControllerBase
{
    private readonly IEstudianteService _svc;
    public EstudianteController(IEstudianteService svc) => _svc = svc;
    private int Id => User.GetUserId();

    [HttpGet("resumen")] public async Task<ResumenEstudianteRow> Resumen() => await _svc.ResumenAsync(Id);
    [HttpGet("perfil")] public async Task<PerfilEstudianteRow> Perfil() => await _svc.PerfilAsync(Id);

    [HttpPut("perfil")]
    public async Task<MensajeResponse> ActualizarPerfil(ActualizarPerfilRequest req)
    {
        await _svc.ActualizarPerfilAsync(Id, req);
        return new MensajeResponse("Perfil actualizado");
    }

    [HttpGet("postulaciones")] public async Task<List<PostulacionEstudianteRow>> Postulaciones() => await _svc.PostulacionesAsync(Id);

    [HttpPost("postulaciones")]
    public async Task<ActionResult<MensajeResponse>> Postular(PostularRequest req) =>
        StatusCode(201, new MensajeResponse("Postulacion enviada", await _svc.PostularAsync(Id, req.IdOferta)));

    [HttpGet("favoritas")] public async Task<List<FavoritaRow>> Favoritas() => await _svc.FavoritasAsync(Id);

    [HttpPost("favoritas/{idOferta:int}")]
    public async Task<IActionResult> GuardarFavorita(int idOferta) { await _svc.GuardarFavoritaAsync(Id, idOferta); return NoContent(); }

    [HttpDelete("favoritas/{idOferta:int}")]
    public async Task<IActionResult> EliminarFavorita(int idOferta) { await _svc.EliminarFavoritaAsync(Id, idOferta); return NoContent(); }

    [HttpGet("hoja-de-vida")] public async Task<HojaVidaMetaRow?> HojaVida() => await _svc.HojaVidaMetaAsync(Id);

    [HttpPut("hoja-de-vida"), RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<HojaVidaMetaRow> SubirHojaVida(IFormFile? archivo) => await _svc.SubirHojaVidaAsync(Id, archivo);

    [HttpGet("hoja-de-vida/descargar")]
    public async Task<IActionResult> DescargarHojaVida()
    {
        var f = await _svc.DescargarHojaVidaAsync(Id);
        return File(f.ArchivoPDF, "application/pdf", f.NombreArchivo);
    }

    [HttpDelete("hoja-de-vida")]
    public async Task<IActionResult> EliminarHojaVida() { await _svc.EliminarHojaVidaAsync(Id); return NoContent(); }
}
