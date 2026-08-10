import http from '../../../shared/api/http';

export const authService = {
  login(credentials) {
    return http.post('/api/auth/login', credentials);
  },

  me() {
    return http.get('/api/auth/me');
  },

  cambiarPassword({ passwordActual, passwordNueva }) {
    return http.put('/api/auth/password', { passwordActual, passwordNueva });
  },

  listarUsuarios() {
    return http.get('/api/auth/usuarios');
  },

  crearUsuario(datos) {
    return http.post('/api/auth/usuarios', datos);
  },

  desactivarUsuario(id) {
    return http.post(`/api/auth/usuarios/${id}/desactivar`);
  },
};
