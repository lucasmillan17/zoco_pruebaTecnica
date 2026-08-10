export function formatearFecha(iso, opciones = {}) {
  if (!iso) return opciones.vacio ?? '—';
  const fecha = new Date(iso);
  if (Number.isNaN(fecha.getTime())) return iso;
  return fecha.toLocaleString('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    ...(opciones.conHora ? { hour: '2-digit', minute: '2-digit' } : {}),
  });
}
