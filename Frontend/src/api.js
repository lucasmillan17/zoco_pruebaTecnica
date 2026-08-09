const API = import.meta.env.VITE_API_URL || 'http://localhost:5000';

async function request(path, options = {}) {
  let response;
  try {
    response = await fetch(`${API}${path}`, {
      headers: { 'Content-Type': 'application/json' },
      ...options,
    });
  } catch {
    throw new Error(`No se pudo conectar con la API (${API}). ¿Está corriendo el backend?`);
  }

  if (response.status === 204) {
    return null;
  }

  let data = null;
  try {
    data = await response.json();
  } catch {
    // respuesta sin cuerpo JSON
  }

  if (!response.ok) {
    const detalle = data?.detail || data?.title || `Error ${response.status}`;
    const erroresDeCampos = data?.errors
      ? Object.values(data.errors).flat().join(' ')
      : '';
    throw new Error(`${detalle}${erroresDeCampos ? ` ${erroresDeCampos}` : ''}`);
  }

  return data;
}

export const api = {
  get: (path) => request(path),
  post: (path, body) => request(path, { method: 'POST', body: JSON.stringify(body) }),
  put: (path, body) => request(path, { method: 'PUT', body: JSON.stringify(body) }),
  del: (path) => request(path, { method: 'DELETE' }),
};
