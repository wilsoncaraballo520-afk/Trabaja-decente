// Cliente de la API. El access token vive SOLO en memoria (nunca en localStorage);
// el refresh token viaja en una cookie HttpOnly que JavaScript no puede leer.
const BASE = '/api';
let token = null;
let session = null;
let inflight = null;

export class ApiError extends Error {
  constructor(status, message, errors) { super(message); this.status = status; this.errors = errors; }
}

const HOME = { Estudiante: '/pages/estudiante/panel.html', Empresa: '/pages/empresa/panel.html', Administrador: '/pages/administrador/panel.html' };
export const homeFor = (rol) => HOME[rol] || '/';
export const getSession = () => session;

function setSession(d) {
  token = d.accessToken;
  session = { rol: d.rol, nombre: d.nombre, estadoAprobacion: d.estadoAprobacion ?? null };
}

async function doRefresh() {
  try {
    const r = await fetch(BASE + '/auth/refresh', { method: 'POST', credentials: 'same-origin', headers: { 'X-Requested-With': 'fetch' } });
    if (!r.ok) { token = null; session = null; return null; }
    setSession(await r.json());
    return session;
  } catch { return null; }
}

/** Renueva el token con la cookie; las llamadas simultáneas comparten una sola petición. */
export function refresh() {
  if (!inflight) inflight = doRefresh().finally(() => { inflight = null; });
  return inflight;
}

function filenameOf(h) {
  if (!h) return 'archivo.pdf';
  const star = /filename\*=UTF-8''([^;]+)/i.exec(h);
  if (star) { try { return decodeURIComponent(star[1]); } catch { /* sigue */ } }
  const plain = /filename="?([^";]+)"?/i.exec(h);
  return plain ? plain[1] : 'archivo.pdf';
}

export async function request(method, path, body, { auth = true, retry = true, blob = false } = {}) {
  const headers = { Accept: 'application/json', 'X-Requested-With': 'fetch' };
  const init = { method, headers, credentials: 'same-origin' };
  if (auth && token) headers.Authorization = 'Bearer ' + token;
  if (body instanceof FormData) init.body = body;
  else if (body !== undefined) { headers['Content-Type'] = 'application/json'; init.body = JSON.stringify(body); }

  let res;
  try { res = await fetch(BASE + path, init); }
  catch { throw new ApiError(0, 'No se pudo conectar con el servidor. Revisa tu conexión.'); }

  if (res.status === 401 && auth && retry) {
    if (await refresh()) return request(method, path, body, { auth, retry: false, blob });
    if (location.pathname.startsWith('/pages/') && !location.pathname.includes('/auth/')) {
      location.href = '/pages/auth/login.html?expirada=1';
    }
  }
  if (!res.ok) {
    let d = null;
    try { d = await res.json(); } catch { /* sin cuerpo */ }
    throw new ApiError(res.status, d?.error || `Error ${res.status}`, d?.errores);
  }
  if (blob) return { blob: await res.blob(), name: filenameOf(res.headers.get('content-disposition')) };
  if (res.status === 204) return null;
  return (res.headers.get('content-type') || '').includes('json') ? res.json() : null;
}

export const api = {
  get: (p, o) => request('GET', p, undefined, o),
  post: (p, b, o) => request('POST', p, b ?? {}, o),
  put: (p, b, o) => request('PUT', p, b, o),
  patch: (p, b, o) => request('PATCH', p, b, o),
  del: (p, o) => request('DELETE', p, undefined, o),
};

export async function login(rol, correo, contrasena) {
  const d = await request('POST', '/auth/login', { rol, correo, contrasena }, { auth: false });
  setSession(d);
  return session;
}

export async function logout() {
  try { await request('POST', '/auth/logout', {}, { auth: false }); } catch { /* da igual */ }
  token = null; session = null;
  location.href = '/';
}

/** Protege una página: exige sesión y rol; si no, manda a iniciar sesión. */
export async function requireRole(rol) {
  const s = await refresh();
  if (!s || s.rol !== rol) { location.replace('/pages/auth/login.html'); return null; }
  return s;
}

/** Descarga un PDF autenticado (el navegador no puede enviar el Bearer en un enlace normal). */
export async function descargar(path) {
  const { blob, name } = await request('GET', path, undefined, { blob: true });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url; a.download = name; document.body.appendChild(a); a.click(); a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 2000);
}
