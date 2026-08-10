export const TOKEN_KEY = 'token';
export const USUARIO_KEY = 'usuario';

export function guardarSesion({ token, usuario }) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USUARIO_KEY, JSON.stringify(usuario));
}

export function limpiarSesion() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USUARIO_KEY);
}

export function leerToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function leerUsuario() {
  try {
    return JSON.parse(localStorage.getItem(USUARIO_KEY));
  } catch {
    return null;
  }
}
