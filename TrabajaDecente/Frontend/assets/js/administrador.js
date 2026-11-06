import { api, requireRole } from '/assets/js/api.js';
import { mountShell } from '/assets/js/shell.js';
import { esc, fmtDate, badge, empty, skeletons, toast, showError } from '/assets/js/ui.js';

const s = await requireRole('Administrador');
if (!s) throw new Error('sin sesión');

const items = [
  { key: 'inicio', icon: 'bi-house', label: 'Inicio' },
  { key: 'empresas', icon: 'bi-building', label: 'Empresas' },
  { key: 'categorias', icon: 'bi-tags', label: 'Categorías' },
];
const head = (t, sub) => `<h1>${esc(t)}</h1><p class="sub">${esc(sub)}</p>`;
const stat = (l, v) => `<div class="stat"><small>${esc(l)}</small><b>${v}</b></div>`;

function empresaActions(e) {
  const a = [];
  if (e.estadoAprobacion !== 'Aprobada') a.push(`<button class="btn btn-green btn-sm" data-ap="${e.idEmpresa}">Aprobar</button>`);
  if (e.estadoAprobacion !== 'Rechazada') a.push(`<button class="btn btn-danger-soft btn-sm" data-rj="${e.idEmpresa}">Rechazar</button>`);
  a.push(`<button class="btn btn-soft btn-sm" data-act="${e.idEmpresa}" data-to="${e.estado ? 0 : 1}">${e.estado ? 'Desactivar' : 'Activar'}</button>`);
  return a.join(' ');
}

function bindEmpresaActions(view, reload) {
  view.onclick = async (ev) => {
    const b = ev.target.closest('[data-ap],[data-rj],[data-act]'); if (!b) return;
    try {
      if (b.dataset.ap) { await api.post(`/admin/empresas/${b.dataset.ap}/aprobar`); toast('Empresa aprobada'); }
      else if (b.dataset.rj) {
        if (!confirm('¿Rechazar esta empresa?')) return;
        await api.post(`/admin/empresas/${b.dataset.rj}/rechazar`); toast('Empresa rechazada');
      } else { await api.patch(`/admin/empresas/${b.dataset.act}/estado`, { activo: b.dataset.to === '1' }); toast('Estado actualizado'); }
      reload();
    } catch (ex) { toast(ex.message, 'err'); }
  };
}

const routes = {
  async inicio(view) {
    const draw = async () => {
      const [r, pend] = await Promise.all([api.get('/admin/resumen'), api.get('/admin/empresas?aprobacion=Pendiente')]);
      view.innerHTML = head('Resumen general', 'Estado actual de la plataforma.')
        + `<div class="stats">${stat('Empresas pendientes', r.empresasPendientes)}${stat('Empresas aprobadas', r.empresasAprobadas)}${stat('Ofertas activas', r.ofertasActivas)}${stat('Estudiantes activos', r.estudiantesActivos)}</div>
        <section class="panel"><h2>Empresas pendientes de aprobación</h2>${pend.length ? `<div class="table-wrap" style="border:0"><table class="td-table"><thead><tr><th>Empresa</th><th>NIT</th><th>Registro</th><th></th></tr></thead><tbody>
          ${pend.map((e) => `<tr><td><b>${esc(e.nombreEmpresa)}</b><br><small class="text-secondary">${esc(e.correoElectronico)}</small></td><td>${esc(e.nit)}</td><td>${esc(fmtDate(e.fechaRegistro))}</td>
            <td class="text-end text-nowrap"><button class="btn btn-green btn-sm" data-ap="${e.idEmpresa}">Aprobar</button> <button class="btn btn-danger-soft btn-sm" data-rj="${e.idEmpresa}">Rechazar</button></td></tr>`).join('')}</tbody></table></div>`
          : empty('bi-check2-circle', 'No hay empresas pendientes')}</section>`;
      bindEmpresaActions(view, draw);
    };
    view.innerHTML = skeletons(2); await draw();
  },

  async empresas(view) {
    let filtro = 'Pendiente';
    const draw = async () => {
      const rows = await api.get('/admin/empresas' + (filtro ? `?aprobacion=${filtro}` : ''));
      const tabs = [['Pendiente', 'Pendientes'], ['Aprobada', 'Aprobadas'], ['Rechazada', 'Rechazadas'], ['', 'Todas']];
      view.innerHTML = head('Empresas', 'Aprueba, rechaza o desactiva cuentas de empresa.')
        + `<div class="tabs">${tabs.map(([v, t]) => `<button data-f="${v}" class="${v === filtro ? 'on' : ''}">${t}</button>`).join('')}</div>`
        + (rows.length ? `<div class="table-wrap"><table class="td-table"><thead><tr><th>Empresa</th><th>NIT</th><th>Teléfono</th><th>Registro</th><th>Aprobación</th><th>Cuenta</th><th></th></tr></thead><tbody>
          ${rows.map((e) => `<tr><td><b>${esc(e.nombreEmpresa)}</b><br><small class="text-secondary">${esc(e.correoElectronico)}</small></td><td>${esc(e.nit)}</td><td>${esc(e.telefono)}</td><td>${esc(fmtDate(e.fechaRegistro))}</td>
            <td>${badge(e.estadoAprobacion)}</td><td>${badge(e.estado ? 'Activa' : 'Inactiva')}</td><td class="text-end text-nowrap">${empresaActions(e)}</td></tr>`).join('')}</tbody></table></div>`
          : empty('bi-building', 'No hay empresas en esta lista'));
      bindEmpresaActions(view, draw);
      view.querySelector('.tabs').addEventListener('click', (e) => { const b = e.target.closest('[data-f]'); if (b) { filtro = b.dataset.f; draw(); } });
    };
    view.innerHTML = skeletons(2); await draw();
  },

  async categorias(view) {
    const draw = async () => {
      const cats = await api.get('/admin/categorias');
      view.innerHTML = head('Categorías', 'Las categorías inactivas no se pueden usar en ofertas nuevas.')
        + `<form class="panel mb-3 d-flex gap-2 flex-wrap align-items-end" id="nc" style="max-width:520px" novalidate>
            <div class="flex-grow-1"><label class="form-label" for="nn">Nueva categoría</label><input class="form-control" id="nn" name="nombreCategoria" required minlength="3" maxlength="100"></div>
            <button class="btn btn-primary">Crear</button></form>
          <div class="alert alert-danger py-2" id="err" hidden role="alert"></div>
          <div class="table-wrap" style="max-width:620px"><table class="td-table"><thead><tr><th>Categoría</th><th>Estado</th><th></th></tr></thead><tbody>
          ${cats.map((c) => `<tr><td>${esc(c.nombreCategoria)}</td><td>${badge(c.estado ? 'Activa' : 'Inactiva')}</td>
            <td class="text-end"><button class="btn btn-soft btn-sm" data-cat="${c.idCategoria}" data-to="${c.estado ? 0 : 1}">${c.estado ? 'Desactivar' : 'Activar'}</button></td></tr>`).join('')}</tbody></table></div>`;
      view.querySelector('#nc').addEventListener('submit', async (e) => {
        e.preventDefault();
        const f = e.target; if (!f.checkValidity()) return f.reportValidity();
        try { await api.post('/admin/categorias', { nombreCategoria: f.nombreCategoria.value }); toast('Categoría creada'); draw(); }
        catch (ex) { showError(view.querySelector('#err'), ex); }
      });
      view.onclick = async (e) => {
        const b = e.target.closest('[data-cat]'); if (!b) return;
        try { await api.patch(`/admin/categorias/${b.dataset.cat}/estado`, { activo: b.dataset.to === '1' }); toast('Estado actualizado'); draw(); }
        catch (ex) { toast(ex.message, 'err'); }
      };
    };
    view.innerHTML = skeletons(2); await draw();
  },
};

await mountShell({ rolLabel: 'Administrador', items, routes });
