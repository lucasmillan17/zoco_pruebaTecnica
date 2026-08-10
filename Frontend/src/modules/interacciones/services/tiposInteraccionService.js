import http from '../../../shared/api/http';

export const tiposInteraccionService = {
  getAll({ pageNumber = 1, pageSize = 100, estadoActivo = 'activos' } = {}) {
    return http.get('/api/tipos-interaccion', { params: { pageNumber, pageSize, estadoActivo } });
  },

  getById(id) {
    return http.get(`/api/tipos-interaccion/${id}`);
  },

  create(payload) {
    return http.post('/api/tipos-interaccion', payload);
  },

  update(id, payload) {
    return http.put(`/api/tipos-interaccion/${id}`, payload);
  },

  remove(id) {
    return http.delete(`/api/tipos-interaccion/${id}`);
  },

  reactivar(id) {
    return http.post(`/api/tipos-interaccion/${id}/reactivar`);
  },
};
