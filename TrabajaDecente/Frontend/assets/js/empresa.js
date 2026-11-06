import { api, requireRole, descargar } from '/assets/js/api.js';
import { mountShell } from '/assets/js/shell.js';
import { esc, pretty, money, fmtDate, badge, empty, skeletons, toast, showError } from '/assets/js/ui.js';

const s = await requireRole('Empresa');
if (!s) throw new Error('sin sesión');

const items = [
  { key: 'inicio', icon: 'bi-house', label: 'Inicio' },
  { key: 'ofertas', icon: 'bi-briefcase', label: 'Mis ofertas', also: ['editar'] },
  { key: 'publicar', icon: 'bi-plus-circle', label: 'Publicar oferta' },
  { key: 'postulaciones', icon: 'bi-people', label: 'Postulaciones' },
];

const head = (t, sub) => `<h1>${esc(t)}</h1><p class="sub">${esc(sub)}</p>`;
const stat = (l, v) => `<div class="stat"><small>${esc(l)}</small><b>${v}</b></div>`;
const TIPOS = [['Laboral', 'Laboral'], ['Practicas', 'Prácticas'], ['Prestacion de servicios', 'Prestación de servicios'], ['Aprendizaje', 'Aprendizaje'], ['Temporal', 'Temporal']];
const MODS = [['Presencial', 'Presencial'], ['Remota', 'Remota'], ['Hibrida', 'Híbrida']];
const aprobada = () => s.estadoAprobacion === 'Aprobada';

function aprobacionBanner() {
  if (aprobada()) return '';
  return s.estadoAprobacion === 'Rechazada'
    ? '<div class="banner bad"><i class="bi bi-x-circle"></i><div><b>Tu empresa fue rechazada.</b> No puedes publicar ofertas. Comunícate con la administración de Trabaja Decente.</div></div>'
    : '<div class="banner warn"><i class="bi bi-hourglass-split"></i><div><b>Tu empresa está pendiente de aprobación.</b> Podrás publicar ofertas cuando un administrador la apruebe.</div></div>';
}

const estadoOferta = (o) => (o.estado === 'Abierta' && new Date(o.fechaCierre) < new Date() ? 'Cerrada' : o.estado);
const toLocalInput = (iso) => (iso || '').slice(0, 16);

async function formOferta(view, id) {
  const [cats, ciudades, mias] = await Promise.all([api.get('/categorias'), api.get('/ciudades'), id ? api.get('/empresa/ofertas') : []]);
  const o = id ? mias.find((x) => x.idOferta === Number(id)) : null;
  if (id && !o) { view.innerHTML = empty('bi-question-circle', 'No encontramos esa oferta'); return; }
  const sel = (arr, v) => arr.map(([val, txt]) => `<option value="${esc(val)}" ${val === v ? 'selected' : ''}>${esc(txt)}</option>`).join('');
  const manana = new Date(Date.now() + 30 * 86400000 - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16);

  view.innerHTML = head(o ? 'Editar oferta' : 'Publicar oferta', o ? 'Al guardar, la oferta queda abierta nuevamente.' : 'Completa los datos de la vacante.') + aprobacionBanner()
    + `<form class="panel" id="of" style="max-width:760px" novalidate><div class="row g-3">
      <div class="col-12"><label class="form-label" for="titulo">Título del cargo</label><input class="form-control" id="titulo" name="titulo" required minlength="5" maxlength="150" value="${esc(o?.titulo)}"></div>
      <div class="col-md-6"><label class="form-label" for="cat">Categoría</label><select class="form-select" id="cat" name="idCategoria" required>${cats.map((c) => `<option value="${c.idCategoria}" ${o?.idCategoria === c.idCategoria ? 'selected' : ''}>${esc(pretty(c.nombreCategoria))}</option>`).join('')}</select></div>
      <div class="col-md-6"><label class="form-label" for="ciu">Ciudad</label><select class="form-select" id="ciu" name="idCiudad" required>${ciudades.map((c) => `<option value="${c.idCiudad}" ${o?.idCiudad === c.idCiudad ? 'selected' : ''}>${esc(pretty(c.nombreCiudad))}</option>`).join('')}</select></div>
      <div class="col-md-6"><label class="form-label" for="tc">Tipo de contrato</label><select class="form-select" id="tc" name="tipoContrato" required>${sel(TIPOS, o?.tipoContrato)}</select></div>
      <div class="col-md-6"><label class="form-label" for="mo">Modalidad</label><select class="form-select" id="mo" name="modalidad" required>${sel(MODS, o?.modalidad)}</select></div>
      <div class="col-md-6"><label class="form-label" for="ho">Horario</label><input class="form-control" id="ho" name="horario" required maxlength="100" placeholder="Lunes a viernes, 8:00 a 17:00" value="${esc(o?.horario)}"></div>
      <div class="col-md-6"><label class="form-label" for="sa">Salario mensual (COP, opcional)</label><input class="form-control" id="sa" name="salario" type="number" min="0" step="50000" value="${o?.salario ?? ''}"></div>
      <div class="col-md-4"><label class="form-label" for="va">Vacantes</label><input class="form-control" id="va" name="vacantes" type="number" min="1" max="1000" required value="${o?.vacantes ?? 1}"></div>
      <div class="col-md-4"><label class="form-label" for="ex">Experiencia mínima (meses)</label><input class="form-control" id="ex" name="experienciaMinimaMeses" type="number" min="0" max="600" required value="${o?.experienciaMinimaMeses ?? 0}"></div>
      <div class="col-md-4"><label class="form-label" for="fc">Fecha de cierre</label><input class="form-control" id="fc" name="fechaCierre" type="datetime-local" required value="${toLocalInput(o?.fechaCierre) || manana}"></div>
      <div class="col-12"><label class="form-label" for="de">Descripción</label><textarea class="form-control" id="de" name="descripcion" rows="6" required minlength="20" maxlength="5000">${esc(o?.descripcion)}</textarea></div>
    </div>
    <div class="alert alert-danger py-2 mt-3" id="err" hidden role="alert"></div>
    <div class="d-flex gap-2 mt-3"><button class="btn btn-primary" ${aprobada() ? '' : 'disabled'}>${o ? 'Guardar cambios' : 'Publicar oferta'}</button><a class="btn btn-soft" href="#ofertas">Cancelar</a></div></form>`;

  const f = view.querySelector('#of');
  f.addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!f.checkValidity()) return f.reportValidity();
    const d = Object.fromEntries(new FormData(f));
    const body = {
      idCategoria: Number(d.idCategoria), idCiudad: Number(d.idCiudad), titulo: d.titulo, descripcion: d.descripcion,
      salario: d.salario === '' ? null : Number(d.salario), tipoContrato: d.tipoContrato, modalidad: d.modalidad, horario: d.horario,
      vacantes: Number(d.vacantes), experienciaMinimaMeses: Number(d.experienciaMinimaMeses),
      fechaCierre: d.fechaCierre.length === 16 ? d.fechaCierre + ':00' : d.fechaCierre,
    };
    try {
      if (o) await api.put(`/empresa/ofertas/${o.idOferta}`, body); else await api.post('/empresa/ofertas', body);
      toast(o ? 'Oferta actualizada' : 'Oferta publicada');
      location.hash = '#ofertas';
    } catch (ex) { showError(view.querySelector('#err'), ex); }
  });
}

const routes = {
  async inicio(view) {
    view.innerHTML = head(`¡Hola, ${s.nombre}!`, 'Resumen de tu actividad en Trabaja Decente.') + skeletons(2);
    const [r, ofertas] = await Promise.all([api.get('/empresa/resumen'), api.get('/empresa/ofertas')]);
    view.innerHTML = head(`¡Hola, ${s.nombre}!`, 'Resumen de tu actividad en Trabaja Decente.') + aprobacionBanner()
      + `<div class="stats">${stat('Ofertas activas', r.ofertasActivas)}${stat('Postulaciones recibidas', r.totalPostulaciones)}${stat('Ofertas publicadas', ofertas.length)}${stat('Estado de la empresa', badge(s.estadoAprobacion || 'Pendiente'))}</div>
      <section class="panel"><h2>Ofertas recientes</h2>${ofertas.length ? ofertas.slice(0, 5).map((o) => `
        <div class="list-row"><div class="grow"><b>${esc(o.titulo)}</b><small>${esc(pretty(o.nombreCiudad))} · cierra el ${esc(fmtDate(o.fechaCierre))}</small></div>
        <small>${o.totalPostulaciones} postulaciones</small>${badge(estadoOferta(o))}</div>`).join('')
        : empty('bi-briefcase', 'Aún no publicas ofertas', 'Publica tu primera vacante para recibir candidatos.')}
        <a class="btn btn-primary mt-3" href="#publicar">Publicar oferta</a></section>`;
  },

  async ofertas(view) {
    const draw = async () => {
      const ofertas = await api.get('/empresa/ofertas');
      view.innerHTML = head('Mis ofertas', 'Edita, cierra o revisa las postulaciones de cada vacante.') + aprobacionBanner()
        + (ofertas.length ? `<div class="table-wrap"><table class="td-table"><thead><tr><th>Cargo</th><th>Ciudad</th><th>Cierre</th><th>Postulaciones</th><th>Estado</th><th></th></tr></thead><tbody>
          ${ofertas.map((o) => `<tr><td><b>${esc(o.titulo)}</b><br><small class="text-secondary">${esc(pretty(o.tipoContrato))} · ${esc(pretty(o.modalidad))} · ${esc(money(o.salario))}</small></td>
            <td>${esc(pretty(o.nombreCiudad))}</td><td>${esc(fmtDate(o.fechaCierre))}</td><td>${o.totalPostulaciones}</td><td>${badge(estadoOferta(o))}</td>
            <td class="text-end text-nowrap"><a class="btn btn-soft btn-sm" href="#postulaciones/${o.idOferta}">Candidatos</a>
              <a class="btn btn-soft btn-sm" href="#editar/${o.idOferta}">Editar</a>
              ${o.estado === 'Abierta' ? `<button class="btn btn-danger-soft btn-sm" data-close="${o.idOferta}">Cerrar</button>` : ''}</td></tr>`).join('')}</tbody></table></div>`
          : empty('bi-briefcase', 'Aún no publicas ofertas', 'Cuando publiques una, aparecerá aquí.'));
      view.onclick = async (e) => {
        const b = e.target.closest('[data-close]'); if (!b) return;
        if (!confirm('¿Cerrar esta oferta? Dejará de recibir postulaciones.')) return;
        try { await api.post(`/empresa/ofertas/${b.dataset.close}/cerrar`); toast('Oferta cerrada'); draw(); } catch (ex) { toast(ex.message, 'err'); }
      };
    };
    await draw();
  },

  publicar: (view) => formOferta(view, null),
  editar: (view, id) => formOferta(view, id),

  async postulaciones(view, idParam) {
    const ofertas = await api.get('/empresa/ofertas');
    let filtro = idParam ? Number(idParam) : '';
    const draw = async () => {
      const rows = await api.get('/empresa/postulaciones' + (filtro ? `?idOferta=${filtro}` : ''));
      view.innerHTML = head('Postulaciones recibidas', 'Revisa candidatos, descarga su hoja de vida y actualiza el estado.')
        + `<div class="mb-3" style="max-width:360px"><label class="form-label" for="fo">Oferta</label><select class="form-select" id="fo"><option value="">Todas las ofertas</option>${ofertas.map((o) => `<option value="${o.idOferta}" ${o.idOferta === filtro ? 'selected' : ''}>${esc(o.titulo)}</option>`).join('')}</select></div>`
        + (rows.length ? `<div class="table-wrap"><table class="td-table"><thead><tr><th>Candidato</th><th>Oferta</th><th>Fecha</th><th>Estado</th><th>Hoja de vida</th></tr></thead><tbody>
          ${rows.map((p) => `<tr><td><b>${esc(p.nombres)} ${esc(p.primerApellido)} ${esc(p.segundoApellido || '')}</b><br><small class="text-secondary">${esc(p.correoElectronico)} · ${esc(p.telefono)}</small></td>
            <td>${esc(p.titulo)}</td><td>${esc(fmtDate(p.fechaPostulacion))}</td>
            <td><label class="sr-only" for="st${p.idPostulacion}">Estado</label><select class="form-select form-select-sm" id="st${p.idPostulacion}" data-st="${p.idPostulacion}" style="min-width:150px">
              ${['Aplicado', 'En proceso', 'Proceso finalizado'].map((e) => `<option ${e === p.estado ? 'selected' : ''}>${e}</option>`).join('')}</select></td>
            <td>${p.tieneHojaDeVida ? `<button class="btn btn-soft btn-sm" data-cv="${p.idPostulacion}"><i class="bi bi-download"></i> PDF</button>` : '<small class="text-secondary">No cargada</small>'}</td></tr>`).join('')}</tbody></table></div>`
          : empty('bi-people', 'Todavía no hay postulaciones', 'Cuando un estudiante se postule, lo verás aquí.'));

      view.querySelector('#fo').addEventListener('change', (e) => { filtro = e.target.value ? Number(e.target.value) : ''; draw(); });
      view.onchange = async (e) => {
        const sel = e.target.closest('[data-st]'); if (!sel) return;
        try { await api.patch(`/empresa/postulaciones/${sel.dataset.st}/estado`, { estado: sel.value }); toast('Estado actualizado'); }
        catch (ex) { toast(ex.message, 'err'); draw(); }
      };
      view.onclick = (e) => {
        const b = e.target.closest('[data-cv]'); if (!b) return;
        descargar(`/empresa/postulaciones/${b.dataset.cv}/hoja-de-vida`).catch((ex) => toast(ex.message, 'err'));
      };
    };
    await draw();
  },
};

await mountShell({ rolLabel: 'Empresa', items, routes });
