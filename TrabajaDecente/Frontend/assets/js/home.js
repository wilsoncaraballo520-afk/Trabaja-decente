import { api, refresh, getSession } from '/assets/js/api.js';
import { renderPublicNav, renderFooter } from '/assets/js/shell.js';
import { esc, pretty, catIcon, offerCard, money, initial, colorOf, empty } from '/assets/js/ui.js';
import { bindOfferActions, loadStudentSets } from '/assets/js/offers.js';

await refresh();               // si hay sesión (cookie), la barra muestra "Mi panel"
renderPublicNav(document.getElementById('nav'), 'inicio');
renderFooter(document.getElementById('foot'));

const ctx = { fav: new Set(), applied: new Set(), onChange: () => {} };
if (getSession()?.rol === 'Estudiante') Object.assign(ctx, await loadStudentSets().catch(() => ({})));

try {
  const [ciudades, cats, ofertas] = await Promise.all([
    api.get('/ciudades', { auth: false }),
    api.get('/categorias?conteo=true', { auth: false }),
    api.get('/ofertas?tamano=6', { auth: false }),
  ]);
  document.getElementById('h-ciu').insertAdjacentHTML('beforeend', ciudades.map((c) => `<option value="${c.idCiudad}">${esc(pretty(c.nombreCiudad))}</option>`).join(''));
  document.getElementById('cnt-ofertas').textContent = ofertas.total;
  document.getElementById('cnt-cats').textContent = cats.length;

  document.getElementById('cats').innerHTML = cats.slice(0, 6).map((c) => `
    <a class="cat" href="/pages/ofertas.html?categoria=${c.idCategoria}"><i class="bi ${catIcon(c.nombreCategoria)}"></i>
      <span><b>${esc(pretty(c.nombreCategoria))}</b><small>${c.totalOfertas} oferta${c.totalOfertas === 1 ? '' : 's'}</small></span></a>`).join('');

  document.getElementById('hero-stack').innerHTML = ofertas.items.slice(0, 3).map((o) => `
    <a class="mini-offer" href="/pages/ofertas.html?q=${encodeURIComponent(o.titulo)}">
      <span class="avatar-co ${colorOf(o.nombreEmpresa)}" aria-hidden="true">${initial(o.nombreEmpresa)}</span>
      <span><b>${esc(o.titulo)}</b><span>${esc(o.nombreEmpresa)} · ${esc(money(o.salario))}</span></span></a>`).join('')
    || '<div class="mini-offer"><span><b>Aún no hay ofertas publicadas</b><span>Vuelve pronto.</span></span></div>';

  const feat = document.getElementById('featured');
  feat.innerHTML = ofertas.items.length
    ? ofertas.items.slice(0, 4).map((o) => offerCard(o, { applied: ctx.applied.has(o.idOferta) })).join('')
    : empty('bi-briefcase', 'Todavía no hay ofertas', 'Las empresas aprobadas publicarán pronto.');
  bindOfferActions(feat, ctx);
} catch (err) {
  document.getElementById('featured').innerHTML = empty('bi-exclamation-circle', 'No pudimos cargar las ofertas', err.message);
}
