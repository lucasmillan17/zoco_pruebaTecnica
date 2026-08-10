import axios from 'axios';

const TOKEN_KEY = 'token';
const USUARIO_KEY = 'usuario';

const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
});

// Adjunta el token JWT a cada request autenticado
http.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) {
    config.headers = config.headers || {};
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

http.interceptors.response.use(
  (response) => response.data,
  (error) => {
    const status = error.response?.status;
    const data = error.response?.data;
    const url = error.config?.url || '';

    // Sesión vencida/inválida: limpiar y volver al login (salvo que ya estemos
    // intentando loguearnos, donde el error se muestra en el formulario).
    if (status === 401 && !url.includes('/auth/login')) {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USUARIO_KEY);
      if (!window.location.pathname.startsWith('/login')) {
        window.location.assign('/login');
      }
    }

    let message;
    if (!error.response) {
      message = `No se pudo conectar con la API (${error.config?.baseURL || http.defaults.baseURL}). ¿Está corriendo el backend?`;
    } else {
      message = data?.detail || data?.title || `Error ${status}`;
      const erroresDeCampos = data?.errors
        ? Object.values(data.errors).flat().join(' ')
        : '';
      if (erroresDeCampos) message += ` ${erroresDeCampos}`;
    }

    return Promise.reject(new Error(message));
  }
);

export default http;
