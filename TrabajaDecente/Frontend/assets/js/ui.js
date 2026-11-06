// Utilidades de interfaz compartidas. Todo texto dinámico pasa por esc() para evitar XSS.
export const esc = (v) => String(v ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

const PRETTY = {
  'Tecnologia y sistemas': 'Tecnología y sistemas', 'Administracion': 'Administración', 'Atencion al cliente': 'Atención al cliente',
  'Diseno y comunicacion': 'Diseño y comunicación', 'Educacion': 'Educación', 'Ingenieria': 'Ingeniería', 'Logistica': 'Logística',
  'Otras areas': 'Otras áreas', 'Medellin': 'Medellín', 'Itagui': 'Itagüí', 'Hibrida': 'Híbrida', 'Practicas': 'Prácticas',
  'Prestacion de servicios': 'Prestación de servicios', 'Laboral': 'Contrato laboral',
};
export const pretty = (s) => PRETTY[s] ?? s;

const ICONS = [['Tecnologia', 'bi-code-slash'], ['Administracion', 'bi-briefcase'], ['Ventas', 'bi-cart3'], ['Atencion', 'bi-headset'],
  ['Diseno', 'bi-palette'], ['Educacion', 'bi-mortarboard'], ['Ingenieria', 'bi-gear'], ['Logistica', 'bi-truck'],
  ['Servicios', 'bi-tools'], ['Contabilidad', 'bi-calculator'], ['Marketing', 'bi-megaphone']];
export const catIcon = (n) => (ICONS.find(([k]) => (n || '').startsWith(k)) || [0, 'bi-grid'])[1];

const COP = new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 });
export const money = (n) => (n == null ? 'Salario a convenir' : COP.format(n));
export const fmtDate = (iso) => (iso ? new Date(iso).toLocaleDateString('es-CO', { day: 'numeric', month: 'short', year: 'numeric' }) : '');

export function timeAgo(iso) {
  const days = Math.floor((Date.now() - new Date(iso).getTime()) / 86400000);
  if (days <= 0) return 'Publicada hoy';
  if (days === 1) return 'Publicada ayer';
  if (days < 30) return `Publicada hace ${days} días`;
  return 'Publicada ' + fmtDate(iso);
}

export const logo = (cls = '') => `
  <a class="brand ${cls}" href="/" aria-label="Trabaja Decente, inicio">
    <svg viewBox="0 0 40 40" aria-hidden="true"><rect x="2" y="9" width="36" height="27" rx="7" fill="#6C5CE7"/><path d="M14 9V7a3 3 0 0 1 3-3h6a3 3 0 0 1 3 3v2" fill="none" stroke="#584FEA" stroke-width="3"/><rect x="2" y="19" width="36" height="4" fill="#584FEA"/><rect x="17" y="17" width="6" height="8" rx="2" fill="#FDCB6E"/></svg>
    <span><span class="brand-name">Trabaja<br>Decente</span><span class="brand-sub">Pascual Bravo</span></span>
  </a>`;

const STATE_CLASS = { 'Aplicado': 'st-aplicado', 'En proceso': 'st-proceso', 'Proceso finalizado': 'st-final', 'Abierta': 'st-abierta', 'Cerrada': 'st-cerrada',
  'Pendiente': 'st-pendiente', 'Aprobada': 'st-aprobada', 'Rechazada': 'st-rechazada', 'Activa': 'st-activa', 'Inactiva': 'st-inactiva' };
export const badge = (estado) => `<span class="st ${STATE_CLASS[estado] || 'st-aplicado'}">${esc(estado)}</span>`;

export const initial = (s) => esc((s || '?').trim().charAt(0).toUpperCase());
export const colorOf = (s) => 'c' + (1 + [...String(s || '')].reduce((a, c) => a + c.charCodeAt(0), 0) % 6);

export const empty = (icon, title, text = '') =>
  `<div class="empty"><i class="bi ${icon}"></i><b>${esc(title)}</b><div>${esc(text)}</div></div>`;
export const skeletons = (n = 3) => Array.from({ length: n }, () => '<div class="skeleton"></div>').join('');

export function toast(msg, type = 'ok') {
  let box = document.getElementById('toasts');
  if (!box) { box = document.createElement('div'); box.id = 'toasts'; box.setAttribute('aria-live', 'polite'); document.body.appendChild(box); }
  const t = document.createElement('div');
  t.className = 'toast-td ' + type;
  t.innerHTML = `<i class="bi ${type === 'ok' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill'}"></i><span>${esc(msg)}</span>`;
  box.appendChild(t);
  setTimeout(() => t.remove(), 4500);
}

export const debounce = (fn, ms = 300) => { let t; return (...a) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); }; };

export function showError(el, err) {
  el.hidden = false;
  el.textContent = err?.message || 'Ocurrió un error inesperado';
}

/** Tarjeta de oferta. Las acciones se resuelven por delegación (data-action), sin handlers en línea. */
export function offerCard(o, { fav = false, canFav = false, applied = false } = {}) {
  return `
  <article class="offer" data-id="${o.idOferta}">
    <div class="offer-top">
      <div class="avatar-co ${colorOf(o.nombreEmpresa)}" aria-hidden="true">${initial(o.nombreEmpresa)}</div>
      <div><h3>${esc(o.titulo)}</h3><div class="co">${esc(o.nombreEmpresa)}</div></div>
      ${canFav ? `<button class="fav-btn ${fav ? 'on' : ''}" data-action="fav" aria-pressed="${fav}" aria-label="${fav ? 'Quitar de favoritas' : 'Guardar en favoritas'}"><i class="bi ${fav ? 'bi-bookmark-fill' : 'bi-bookmark'}"></i></button>` : ''}
    </div>
    <div class="chips">
      <span class="chip">${esc(pretty(o.modalidad))}</span>
      <span class="chip g">${esc(pretty(o.tipoContrato))}</span>
      <span class="chip n"><i class="bi bi-geo-alt"></i> ${esc(pretty(o.nombreCiudad))}</span>
    </div>
    <div class="salary">${esc(money(o.salario))}</div>
    <div class="foot"><span>${esc(timeAgo(o.fechaPublicacion))}</span>
      ${applied ? '<span class="st st-aplicado">Ya postulado</span>' : '<button class="btn btn-soft btn-sm" data-action="detalle">Ver oferta</button>'}
    </div>
  </article>`;
}

export function pagerHtml(pagina, total) {
  if (total <= 1) return '';
  const nums = [];
  for (let p = Math.max(1, pagina - 2); p <= Math.min(total, pagina + 2); p++) nums.push(p);
  return `<button data-page="${pagina - 1}" ${pagina === 1 ? 'disabled' : ''} aria-label="Página anterior"><i class="bi bi-chevron-left"></i></button>`
    + nums.map((p) => `<button data-page="${p}" class="${p === pagina ? 'on' : ''}" ${p === pagina ? 'aria-current="page"' : ''}>${p}</button>`).join('')
    + `<button data-page="${pagina + 1}" ${pagina === total ? 'disabled' : ''} aria-label="Página siguiente"><i class="bi bi-chevron-right"></i></button>`;
}

export const passwordOk = (p) => /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,72}$/.test(p);
