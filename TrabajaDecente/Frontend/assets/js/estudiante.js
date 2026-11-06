import { api, requireRole, descargar } from '/assets/js/api.js';
import { mountShell } from '/assets/js/shell.js';
import { mountOffers, bindOfferActions, loadStudentSets } from '/assets/js/offers.js';
import { esc, pretty, money, fmtDate, badge, offerCard, empty, skeletons, toast, initial, colorOf, showError } from '/assets/js/ui.js';

const s = await requireRole('Estudiante');
if (!s) throw new Error('sin sesión');

const items = [
  { key: 'inicio', icon: 'bi-house', label: 'Inicio' },
  { key: 'buscar', icon: 'bi-search', label: 'Buscar ofertas' },
  { key: 'postulaciones', icon: 'bi-send', label: 'Mis postulaciones' },
  { key: 'favoritos', icon: 'bi-bookmark', label: 'Favoritos' },
  { key: 'hoja-de-vida', icon: 'bi-file-earmark-person', label: 'Mi hoja de vida' },
  { key: 'perfil', icon: 'bi-person', label: 'Mi perfil' },
];

const head = (t, sub) => `<h1>${esc(t)}</h1><p class="sub">${esc(sub)}</p>`;
const statCard = (label, value) => `<div class="stat"><small>${esc(label)}</small><b>${value}</b></div>`;

const routes = {
  async inicio(view) {
    view.innerHTML = head(`¡Hola, ${s.nombre}!`, 'Este es el resumen de tu búsqueda de empleo.') + skeletons(2);
    const [r, posts, ofertas] = await Promise.all([api.get('/estudiante/resumen'), api.get('/estudiante/postulaciones'), api.get('/ofertas?tamano=5')]);
    const applied = new Set(posts.map((p) => p.idOferta));
    view.innerHTML = head(`¡Hola, ${s.nombre}!`, 'Este es el resumen de tu búsqueda de empleo.')
      + (r.tieneHojaDeVida ? '' : '<div class="banner warn"><i class="bi bi-exclamation-triangle"></i><div><b>Aún no subes tu hoja de vida.</b> Las empresas la revisan cuando te postulas. <a href="#hoja-de-vida">Subirla ahora</a></div></div>')
      + `<div class="stats">${statCard('Postulaciones enviadas', r.totalPostulaciones)}${statCard('En proceso', r.enProceso)}${statCard('Favoritas guardadas', r.totalFavoritas)}${statCard('Hoja de vida', r.tieneHojaDeVida ? 'Cargada' : 'Pendiente')}</div>
      <div class="cols">
        <section class="panel"><h2>Postulaciones recientes</h2>${posts.length ? posts.slice(0, 4).map((p) => `
          <div class="list-row"><span class="avatar-co ${colorOf(p.nombreEmpresa)}">${initial(p.nombreEmpresa)}</span>
            <div class="grow"><b>${esc(p.titulo)}</b><small>${esc(p.nombreEmpresa)} · ${esc(fmtDate(p.fechaPostulacion))}</small></div>${badge(p.estadoPostulacion)}</div>`).join('')
          : empty('bi-send', 'Aún no te has postulado', 'Busca una oferta y postúlate en un clic.')}
          <a class="d-block mt-2 small" href="#postulaciones">Ver todas mis postulaciones</a></section>
        <section class="panel"><h2>Ofertas nuevas</h2>${ofertas.items.length ? ofertas.items.filter((o) => !applied.has(o.idOferta)).slice(0, 4).map((o) => `
          <div class="list-row"><span class="avatar-co ${colorOf(o.nombreEmpresa)}">${initial(o.nombreEmpresa)}</span>
            <div class="grow"><b>${esc(o.titulo)}</b><small>${esc(o.nombreEmpresa)} · ${esc(money(o.salario))}</small></div></div>`).join('') || '<p class="text-secondary small mb-0">Ya te postulaste a todas las ofertas recientes.</p>'
          : empty('bi-briefcase', 'Sin ofertas por ahora')}
          <a class="d-block mt-2 small" href="#buscar">Explorar todas las ofertas</a></section>
      </div>`;
  },

  async buscar(view) {
    view.innerHTML = head('Buscar ofertas', 'Filtra por categoría, ciudad, modalidad o salario.') + '<div id="root"></div>';
    await mountOffers(view.querySelector('#root'));
  },

  async postulaciones(view) {
    view.innerHTML = head('Mis postulaciones', 'Sigue el estado de cada proceso.') + skeletons(2);
    const posts = await api.get('/estudiante/postulaciones');
    const tabs = ['Todas', 'Aplicado', 'En proceso', 'Proceso finalizado'];
    let tab = 'Todas';
    const draw = () => {
      const list = posts.filter((p) => tab === 'Todas' || p.estadoPostulacion === tab);
      view.innerHTML = head('Mis postulaciones', 'Sigue el estado de cada proceso.')
        + `<div class="tabs" role="tablist">${tabs.map((t) => `<button class="${t === tab ? 'on' : ''}" data-tab="${esc(t)}" role="tab" aria-selected="${t === tab}">${esc(t)}</button>`).join('')}</div>`
        + (list.length ? `<div class="panel">${list.map((p) => `
          <div class="list-row"><span class="avatar-co ${colorOf(p.nombreEmpresa)}">${initial(p.nombreEmpresa)}</span>
            <div class="grow"><b>${esc(p.titulo)}</b><small>${esc(p.nombreEmpresa)} · ${esc(pretty(p.nombreCiudad))} · ${esc(pretty(p.modalidad))} · ${esc(money(p.salario))}</small><br><small>Postulado el ${esc(fmtDate(p.fechaPostulacion))}</small></div>
            ${badge(p.estadoPostulacion)}</div>`).join('')}</div>`
          : empty('bi-send', 'No hay postulaciones en este estado', 'Cuando te postules, las verás aquí.'));
    };
    view.onclick = (e) => { const b = e.target.closest('[data-tab]'); if (b) { tab = b.dataset.tab; draw(); } };
    draw();
  },

  async favoritos(view) {
    const draw = async () => {
      const [favs, sets] = await Promise.all([api.get('/estudiante/favoritas'), loadStudentSets()]);
      view.innerHTML = head('Favoritos', 'Ofertas que guardaste para revisar después.')
        + (favs.length ? `<div class="offer-grid" id="fav-list">${favs.map((o) => offerCard(o, { fav: true, canFav: true, applied: sets.applied.has(o.idOferta) })).join('')}</div>`
          : empty('bi-bookmark', 'Todavía no guardas ofertas', 'Toca el marcador de una oferta para guardarla aquí.'));
      const list = view.querySelector('#fav-list');
      if (list) bindOfferActions(list, { ...sets, onChange: draw });
    };
    await draw();
  },

  async 'hoja-de-vida'(view) {
    const draw = async () => {
      const cv = await api.get('/estudiante/hoja-de-vida');
      view.innerHTML = head('Mi hoja de vida', 'Un solo archivo PDF de hasta 5 MB. Solo las empresas a las que te postulas pueden verlo.')
        + `<div class="row g-3"><div class="col-lg-6"><div class="panel">
          <h2>Archivo actual</h2>
          ${cv ? `<div class="list-row"><i class="bi bi-file-earmark-pdf fs-2 text-danger"></i><div class="grow"><b>${esc(cv.nombreArchivo)}</b><small>Actualizada el ${esc(fmtDate(cv.fechaCarga))} · ${(cv.tamanoBytes / 1048576).toFixed(2)} MB</small></div></div>
            <div class="d-flex gap-2 mt-3"><button class="btn btn-soft" id="dl"><i class="bi bi-download"></i> Descargar</button><button class="btn btn-danger-soft" id="rm"><i class="bi bi-trash"></i> Eliminar</button></div>`
            : empty('bi-file-earmark-person', 'Aún no has subido tu hoja de vida')}
        </div></div>
        <div class="col-lg-6"><div class="upload"><i class="bi bi-cloud-arrow-up"></i>
          <h2 class="h6 mt-2">${cv ? 'Reemplazar hoja de vida' : 'Subir hoja de vida'}</h2>
          <label class="sr-only" for="pdf">Archivo PDF</label>
          <input class="form-control mb-2" type="file" id="pdf" accept="application/pdf,.pdf">
          <div class="alert alert-danger py-2" id="err" hidden role="alert"></div>
          <button class="btn btn-primary" id="up">Subir PDF</button>
          <div class="hint mt-2">Solo PDF. Máximo 5 MB.</div></div></div></div>`;

      view.querySelector('#dl')?.addEventListener('click', () => descargar('/estudiante/hoja-de-vida/descargar').catch((e) => toast(e.message, 'err')));
      view.querySelector('#rm')?.addEventListener('click', async () => {
        if (!confirm('¿Eliminar tu hoja de vida?')) return;
        try { await api.del('/estudiante/hoja-de-vida'); toast('Hoja de vida eliminada'); draw(); } catch (e) { toast(e.message, 'err'); }
      });
      view.querySelector('#up').addEventListener('click', async (e) => {
        const err = view.querySelector('#err'); err.hidden = true;
        const file = view.querySelector('#pdf').files[0];
        if (!file) return showError(err, new Error('Selecciona un archivo PDF.'));
        if (!/\.pdf$/i.test(file.name)) return showError(err, new Error('Solo se permiten archivos PDF.'));
        if (file.size > 5 * 1024 * 1024) return showError(err, new Error('El archivo supera 5 MB.'));
        const fd = new FormData(); fd.append('archivo', file);
        e.target.disabled = true;
        try { await api.put('/estudiante/hoja-de-vida', fd); toast('Hoja de vida guardada'); await draw(); }
        catch (ex) { showError(err, ex); e.target.disabled = false; }
      });
    };
    await draw();
  },

  async perfil(view) {
    const p = await api.get('/estudiante/perfil');
    view.innerHTML = head('Mi perfil', 'Puedes actualizar tu nombre y teléfono. El documento y el correo no se pueden cambiar.')
      + `<form class="panel" id="pf" style="max-width:640px" novalidate>
        <div class="row g-3">
          <div class="col-md-6"><label class="form-label">Documento</label><input class="form-control" value="${esc(p.tipoDocumento)} ${esc(p.numeroDocumento)}" disabled></div>
          <div class="col-md-6"><label class="form-label">Correo electrónico</label><input class="form-control" value="${esc(p.correoElectronico)}" disabled></div>
          <div class="col-12"><label class="form-label" for="nombres">Nombres</label><input class="form-control" id="nombres" name="nombres" required maxlength="100" value="${esc(p.nombres)}"></div>
          <div class="col-md-6"><label class="form-label" for="pa">Primer apellido</label><input class="form-control" id="pa" name="primerApellido" required maxlength="50" value="${esc(p.primerApellido)}"></div>
          <div class="col-md-6"><label class="form-label" for="sa">Segundo apellido</label><input class="form-control" id="sa" name="segundoApellido" maxlength="50" value="${esc(p.segundoApellido || '')}"></div>
          <div class="col-md-6"><label class="form-label" for="tel">Teléfono</label><input class="form-control" id="tel" name="telefono" required pattern="\\+?[0-9 ]{7,18}" value="${esc(p.telefono)}"></div>
          <div class="col-md-6"><label class="form-label">Miembro desde</label><input class="form-control" value="${esc(fmtDate(p.fechaRegistro))}" disabled></div>
        </div>
        <div class="alert alert-danger py-2 mt-3" id="err" hidden role="alert"></div>
        <button class="btn btn-primary mt-3">Guardar cambios</button></form>`;
    const f = view.querySelector('#pf');
    f.addEventListener('submit', async (e) => {
      e.preventDefault();
      if (!f.checkValidity()) return f.reportValidity();
      try { await api.put('/estudiante/perfil', Object.fromEntries(new FormData(f))); toast('Perfil actualizado'); }
      catch (ex) { showError(view.querySelector('#err'), ex); }
    });
  },
};

await mountShell({ rolLabel: 'Estudiante', items, routes });
