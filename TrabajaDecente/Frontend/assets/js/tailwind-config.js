// Tailwind (Play CDN) convive con Bootstrap: prefijo "tw-" y sin preflight para no pisar sus estilos.
window.tailwind = window.tailwind || {};
window.tailwind.config = {
  prefix: 'tw-',
  corePlugins: { preflight: false },
  theme: { extend: { colors: { brand: { DEFAULT: '#6C5CE7', dark: '#584FEA' } } } }
};
