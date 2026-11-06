import { logo, esc, initial } from '/assets/js/ui.js';
import { logout, getSession } from '/assets/js/api.js';

/** Barra pública (inicio y ofertas). */
export function renderPublicNav(el, activo = '') {
  const s = getSession();
  const homes = { Estudiante: '/pages/estudiante/panel.html', Empresa: '/pages/empresa/panel.html', Administrador: '/pages/administrador/panel.html' };
  const link = (href, txt, key) => `<a href="${href}" class="${activo === key ? 'active' : ''}">${txt}</a>`;
  el.innerHTML = `
    <header class="td-nav"><div class="container-td wrap">
      ${logo()}
      <nav aria-label="Principal">${link('/', 'Inicio', 'inicio')}${link('/pages/ofertas.html', 'Ofertas', 'ofertas')}${link('/#empresas', 'Empresas', 'empresas')}${link('/#como-funciona', 'Cómo funciona', 'como')}</nav>
      <div class="actions">${s
        ? `<a class="btn btn-primary" href="${homes[s.rol]}">Mi panel</a>`
        : '<a class="btn btn-outline-primary" href="/pages/auth/login.html">Iniciar sesión</a><a class="btn btn-primary d-none d-sm-inline-block" href="/pages/auth/registro-estudiante.html">Registrarse</a>'}</div>
    </div></header>`;
}

export function renderFooter(el) {
  el.innerHTML = `<footer class="td-foot"><div class="container-td d-flex flex-wrap gap-3 justify-content-between">
    <span>© ${new Date().getFullYear()} Trabaja Decente · Institución Universitaria Pascual Bravo</span>
    <span>Ofertas para el Área Metropolitana del Valle de Aburrá</span></div></footer>`;
}

/**
 * Estructura de los paneles: barra lateral + enrutador por hash.
 * items: [{ key, icon, label }]   routes: { key: (view, param) => Promise|void }
 */
export function mountShell({ rolLabel, items, routes, defaultRoute = 'inicio' }) {
  const s = getSession();
  document.body.classList.add('app-body');
  document.getElementById('app').innerHTML = `
    <div class="shell">
      <aside class="sidebar" id="sidebar" aria-label="Menú del panel">
        ${logo()}
        <div class="role-tag">${esc(rolLabel)}</div>
        <nav>${items.map((i) => `<a href="#${i.key}" data-key="${i.key}"><i class="bi ${i.icon}"></i>${esc(i.label)}</a>`).join('')}</nav>
        <button class="logout" id="logout"><i class="bi bi-box-arrow-left"></i>Cerrar sesión</button>
      </aside>
      <div>
        <header class="topbar">
          <button class="menu-btn" id="menu" aria-label="Abrir menú"><i class="bi bi-list"></i></button>
          <div class="topbar-title" id="top-title"></div>
          <div class="who"><span class="who-avatar" aria-hidden="true">${initial(s.nombre)}</span><span class="d-none d-sm-inline">${esc(s.nombre)}</span></div>
        </header>
        <main id="view" class="view" tabindex="-1"></main>
      </div>
    </div>`;

  const sidebar = document.getElementById('sidebar');
  const view = document.getElementById('view');
  document.getElementById('logout').addEventListener('click', logout);
  document.getElementById('menu').addEventListener('click', () => sidebar.classList.toggle('open'));

  async function route() {
    const [key, param] = (location.hash.slice(1) || defaultRoute).split('/');
    const handler = routes[key] ? key : defaultRoute;
    sidebar.classList.remove('open');
    sidebar.querySelectorAll('nav a').forEach((a) => a.classList.toggle('active', a.dataset.key === (items.find((i) => i.key === handler || i.also?.includes(handler))?.key)));
    const item = items.find((i) => i.key === handler);
    document.getElementById('top-title').textContent = item ? item.label : '';
    view.innerHTML = ''; view.onclick = null;
    try { await routes[handler](view, param); }
    catch (err) { view.innerHTML = `<div class="empty"><i class="bi bi-exclamation-circle"></i><b>No pudimos cargar esta sección</b><div>${esc(err.message)}</div></div>`; }
    view.focus({ preventScroll: true });
  }
  window.addEventListener('hashchange', route);
  return route();
}
