import { login, api, homeFor, refresh } from '/assets/js/api.js';
import { passwordOk, showError } from '/assets/js/ui.js';

const page = document.body.dataset.page;
const form = document.getElementById('form');
const err = document.getElementById('err');
const submit = document.getElementById('submit');
const params = new URLSearchParams(location.search);

// Mostrar / ocultar contraseña
document.querySelectorAll('[data-toggle-pass]').forEach((b) => b.addEventListener('click', () => {
  const input = b.parentElement.querySelector('input');
  const show = input.type === 'password';
  input.type = show ? 'text' : 'password';
  b.querySelector('i').className = show ? 'bi bi-eye-slash' : 'bi bi-eye';
  b.setAttribute('aria-label', show ? 'Ocultar contraseña' : 'Mostrar contraseña');
}));

// Solo se aceptan rutas internas del sitio como destino tras el login (evita redirecciones abiertas).
const safeNext = (n) => (n && /^\/[^/\\]/.test(n) && !n.includes('//') ? n : null);

async function busy(fn) {
  err.hidden = true; submit.disabled = true;
  try { await fn(); } catch (e) { showError(err, e); } finally { submit.disabled = false; }
}

if (page === 'login') {
  if (params.get('registro') === 'ok') { const ok = document.getElementById('ok'); ok.hidden = false; ok.textContent = 'Cuenta creada. Ya puedes iniciar sesión.'; }
  if (params.get('registro') === 'empresa') { const ok = document.getElementById('ok'); ok.hidden = false; ok.textContent = 'Empresa registrada. Un administrador debe aprobarla antes de que puedas publicar ofertas.'; }
  if (params.get('expirada')) { const w = document.getElementById('warn'); w.hidden = false; w.textContent = 'Tu sesión terminó. Inicia sesión de nuevo.'; }
  const s = await refresh();            // si ya hay sesión, entra directo al panel
  if (s) location.replace(homeFor(s.rol));

  form.addEventListener('submit', (e) => {
    e.preventDefault();
    if (!form.checkValidity()) { form.reportValidity(); return; }
    busy(async () => {
      const f = new FormData(form);
      const ses = await login(f.get('rol'), f.get('correo'), f.get('contrasena'));
      location.href = safeNext(params.get('next')) || homeFor(ses.rol);
    });
  });
} else {
  // Registro en dos pasos
  const steps = [...form.querySelectorAll('[data-step]')];
  const dots = [...document.querySelectorAll('#stepper > span:not(.bar)')];
  let current = 0;
  const go = (n) => {
    current = n;
    steps.forEach((s, i) => { s.hidden = i !== n; });
    dots.forEach((d, i) => d.classList.toggle('on', i <= n));
    steps[n].querySelector('input,select')?.focus();
  };
  form.querySelector('[data-next]').addEventListener('click', () => {
    const fields = [...steps[0].querySelectorAll('input,select')];
    const bad = fields.find((f) => !f.checkValidity());
    if (bad) { bad.reportValidity(); return; }
    go(1);
  });
  form.querySelector('[data-prev]').addEventListener('click', () => go(0));

  form.addEventListener('submit', (e) => {
    e.preventDefault();
    const bad = [...form.querySelectorAll('input,select')].find((f) => !f.checkValidity());
    if (bad) { if (steps[0].contains(bad)) go(0); bad.reportValidity(); return; }
    const p1 = form.elements.contrasena.value;
    if (!passwordOk(p1)) { showError(err, new Error('La contraseña debe tener 8 a 72 caracteres, con mayúscula, minúscula y número.')); return; }
    if (p1 !== document.getElementById('pass2').value) { showError(err, new Error('Las contraseñas no coinciden.')); return; }

    const data = Object.fromEntries(new FormData(form));
    busy(async () => {
      if (page === 'reg-estudiante') {
        await api.post('/auth/registro/estudiante', data, { auth: false });
        location.href = '/pages/auth/login.html?registro=ok';
      } else {
        await api.post('/auth/registro/empresa', data, { auth: false });
        location.href = '/pages/auth/login.html?registro=empresa';
      }
    });
  });
}
