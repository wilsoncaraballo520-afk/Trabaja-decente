import { refresh } from '/assets/js/api.js';
import { renderPublicNav, renderFooter } from '/assets/js/shell.js';
import { mountOffers } from '/assets/js/offers.js';

await refresh();
renderPublicNav(document.getElementById('nav'), 'ofertas');
renderFooter(document.getElementById('foot'));
const p = new URLSearchParams(location.search);
await mountOffers(document.getElementById('root'), {
  prefill: { q: p.get('q'), categoria: p.get('categoria'), ciudad: p.get('ciudad'), modalidad: p.get('modalidad') },
});
