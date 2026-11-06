import { api, getSession } from '/assets/js/api.js';
import { esc, pretty, money, fmtDate, offerCard, pagerHtml, empty, skeletons, toast, debounce } from '/assets/js/ui.js';

let modalEl = null;
function ensureModal() {
  if (modalEl) return modalEl;
  modalEl = document.createElement('div');
  modalEl.className = 'modal fade';
  modalEl.tabIndex = -1;
  modalEl.innerHTML = '<div class="modal-dialog modal-dialog-centered modal-dialog-scrollable modal-lg"><div class="modal-content" style="border-radius:18px"></div></div>';
  document.body.appendChild(modalEl);
  return modalEl;
}

/**
 * Conecta tarjetas de oferta con: ver detalle, postularse y guardar en favoritas.
 * ctx = { fav:Set, applied:Set, onChange?:fn }  (los sets solo se usan con sesión de estudiante)
 */
export function bindOfferActions(root, ctx) {
  root.addEventListener('click', async (e) => {
    const btn = e.target.closest('[data-action]');
    const card = e.target.closest('.offer');
    if (!btn || !card) return;
    const id = Number(card.dataset.id);
    if (btn.dataset.action === 'detalle') openDetail(id, ctx);
    if (btn.dataset.action === 'fav') await toggleFav(id, ctx);
  });
}

async function toggleFav(id, ctx) {
  try {
    if (ctx.fav.has(id)) { await api.del(`/estudiante/favoritas/${id}`); ctx.fav.delete(id); toast('Quitada de favoritas'); }
    else { await api.post(`/estudiante/favoritas/${id}`); ctx.fav.add(id); toast('Guardada en favoritas'); }
    ctx.onChange?.();
  } catch (err) { toast(err.message, 'err'); }
}

export async function openDetail(id, ctx) {
  const el = ensureModal();
  const content = el.querySelector('.modal-content');
  content.innerHTML = `<div class="modal-body p-4">${skeletons(1)}</div>`;
  const modal = window.bootstrap.Modal.getOrCreateInstance(el);
  modal.show();
  try {
    const o = await api.get(`/ofertas/${id}`, { auth: false });
    const s = getSession();
    const student = s?.rol === 'Estudiante';
    const applied = ctx.applied?.has(id);
    content.innerHTML = `
      <div class="modal-header border-0 pb-0"><div><h2 class="h4 mb-1">${esc(o.titulo)}</h2><div class="text-secondary">${esc(o.nombreEmpresa)} · ${esc(pretty(o.nombreCiudad))}</div></div>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button></div>
      <div class="modal-body">
        <div class="chips"><span class="chip">${esc(pretty(o.modalidad))}</span><span class="chip g">${esc(pretty(o.tipoContrato))}</span><span class="chip y">${esc(pretty(o.nombreCategoria))}</span></div>
        <div class="detail-grid">
          <div><small>Salario</small><b>${esc(money(o.salario))}</b></div>
          <div><small>Horario</small><b>${esc(o.horario)}</b></div>
          <div><small>Vacantes</small><b>${o.vacantes}</b></div>
          <div><small>Experiencia mínima</small><b>${o.experienciaMinimaMeses ? o.experienciaMinimaMeses + ' meses' : 'Sin experiencia'}</b></div>
          <div><small>Publicada</small><b>${esc(fmtDate(o.fechaPublicacion))}</b></div>
          <div><small>Cierra</small><b>${esc(fmtDate(o.fechaCierre))}</b></div>
        </div>
        <h3 class="h6">Descripción</h3><p class="desc">${esc(o.descripcion)}</p>
      </div>
      <div class="modal-footer border-0">
        ${student ? `<button class="btn btn-soft" data-m="fav"><i class="bi ${ctx.fav.has(id) ? 'bi-bookmark-fill' : 'bi-bookmark'}"></i> ${ctx.fav.has(id) ? 'Guardada' : 'Guardar'}</button>` : ''}
        ${applied ? '<span class="st st-aplicado">Ya te postulaste</span>'
          : (!s || student ? '<button class="btn btn-primary" data-m="postular">Postularme</button>' : '')}
      </div>`;

    content.querySelector('[data-m="postular"]')?.addEventListener('click', async (ev) => {
      if (!student) { location.href = '/pages/auth/login.html?next=' + encodeURIComponent(location.pathname + location.search); return; }
      ev.target.disabled = true;
      try {
        await api.post('/estudiante/postulaciones', { idOferta: id });
        ctx.applied.add(id); toast('¡Postulación enviada!'); modal.hide(); ctx.onChange?.();
      } catch (err) { toast(err.message, 'err'); ev.target.disabled = false; }
    });
    content.querySelector('[data-m="fav"]')?.addEventListener('click', async () => { await toggleFav(id, ctx); modal.hide(); });
  } catch (err) {
    content.innerHTML = `<div class="modal-body p-4">${empty('bi-exclamation-circle', 'No pudimos cargar la oferta', err.message)}</div>`;
  }
}

/** Carga favoritas y postulaciones del estudiante para pintar botones correctos. */
export async function loadStudentSets() {
  const [fav, post] = await Promise.all([api.get('/estudiante/favoritas'), api.get('/estudiante/postulaciones')]);
  return { fav: new Set(fav.map((o) => o.idOferta)), applied: new Set(post.map((p) => p.idOferta)) };
}

/** Buscador de ofertas con filtros, orden y paginación (público y panel del estudiante). */
export async function mountOffers(root, { prefill = {} } = {}) {
  const s = getSession();
  const student = s?.rol === 'Estudiante';
  const ctx = { fav: new Set(), applied: new Set(), onChange: () => load() };
  if (student) Object.assign(ctx, await loadStudentSets());

  root.innerHTML = `
    <form class="filters" id="of-form" role="search">
      <div class="row">
        <div class="col-12 col-md-4"><label class="sr-only" for="f-q">Buscar</label><input id="f-q" name="q" class="form-control" maxlength="150" placeholder="Cargo, palabra clave o empresa"></div>
        <div class="col-6 col-md-2"><label class="sr-only" for="f-cat">Categoría</label><select id="f-cat" name="categoria" class="form-select"><option value="">Categoría</option></select></div>
        <div class="col-6 col-md-2"><label class="sr-only" for="f-ciu">Ciudad</label><select id="f-ciu" name="ciudad" class="form-select"><option value="">Ciudad</option></select></div>
        <div class="col-6 col-md-2"><label class="sr-only" for="f-mod">Modalidad</label><select id="f-mod" name="modalidad" class="form-select"><option value="">Modalidad</option><option value="Presencial">Presencial</option><option value="Remota">Remota</option><option value="Hibrida">Híbrida</option></select></div>
        <div class="col-6 col-md-2"><label class="sr-only" for="f-tip">Contrato</label><select id="f-tip" name="tipoContrato" class="form-select"><option value="">Contrato</option><option value="Laboral">Laboral</option><option value="Practicas">Prácticas</option><option value="Prestacion de servicios">Prestación de servicios</option><option value="Aprendizaje">Aprendizaje</option><option value="Temporal">Temporal</option></select></div>
        <div class="col-6 col-md-3"><label class="sr-only" for="f-min">Salario mínimo</label><input id="f-min" name="salarioMin" type="number" min="0" step="50000" class="form-control" placeholder="Salario mínimo"></div>
        <div class="col-6 col-md-3"><label class="sr-only" for="f-ord">Ordenar</label><select id="f-ord" name="orden" class="form-select"><option value="recientes">Más recientes</option><option value="salario_desc">Mayor salario</option><option value="salario_asc">Menor salario</option><option value="cierre">Cierran pronto</option></select></div>
        <div class="col-12 col-md-6 d-flex gap-2 justify-content-md-end"><button type="reset" class="btn btn-soft" id="f-reset">Limpiar</button><button class="btn btn-primary">Buscar</button></div>
      </div>
    </form>
    <div class="text-secondary small mb-2" id="of-meta" aria-live="polite"></div>
    <div class="offer-grid" id="of-list"></div>
    <nav class="pager" id="of-pager" aria-label="Paginación"></nav>`;

  const $ = (id) => root.querySelector(id);
  const form = $('#of-form');
  let pagina = 1;

  const [cats, ciudades] = await Promise.all([api.get('/categorias', { auth: false }), api.get('/ciudades', { auth: false })]);
  $('#f-cat').insertAdjacentHTML('beforeend', cats.map((c) => `<option value="${c.idCategoria}">${esc(pretty(c.nombreCategoria))}</option>`).join(''));
  $('#f-ciu').insertAdjacentHTML('beforeend', ciudades.map((c) => `<option value="${c.idCiudad}">${esc(pretty(c.nombreCiudad))}</option>`).join(''));
  for (const [k, v] of Object.entries(prefill)) { const el = form.elements[k]; if (el && v) el.value = v; }

  async function load() {
    const params = new URLSearchParams();
    new FormData(form).forEach((v, k) => { if (String(v).trim()) params.set(k, String(v).trim()); });
    params.set('pagina', pagina); params.set('tamano', 9);
    $('#of-list').innerHTML = skeletons(3);
    try {
      const r = await api.get('/ofertas?' + params, { auth: false });
      $('#of-meta').textContent = r.total ? `${r.total} oferta${r.total === 1 ? '' : 's'} encontrada${r.total === 1 ? '' : 's'}` : '';
      $('#of-list').innerHTML = r.items.length
        ? r.items.map((o) => offerCard(o, { fav: ctx.fav.has(o.idOferta), canFav: student, applied: ctx.applied.has(o.idOferta) })).join('')
        : empty('bi-search', 'No hay ofertas con esos filtros', 'Prueba con otra palabra o quita algún filtro.');
      $('#of-pager').innerHTML = pagerHtml(r.pagina, r.totalPaginas);
    } catch (err) { $('#of-list').innerHTML = empty('bi-exclamation-circle', 'No pudimos cargar las ofertas', err.message); }
  }

  form.addEventListener('submit', (e) => { e.preventDefault(); pagina = 1; load(); });
  form.addEventListener('reset', () => setTimeout(() => { pagina = 1; load(); }));
  $('#f-q').addEventListener('input', debounce(() => { pagina = 1; load(); }, 450));
  for (const n of ['categoria', 'ciudad', 'modalidad', 'tipoContrato', 'orden']) form.elements[n].addEventListener('change', () => { pagina = 1; load(); });
  $('#of-pager').addEventListener('click', (e) => {
    const b = e.target.closest('[data-page]'); if (!b || b.disabled) return;
    pagina = Number(b.dataset.page); load(); root.scrollIntoView({ behavior: 'smooth', block: 'start' });
  });
  bindOfferActions(root, ctx);
  await load();
}
