import http from '../../../shared/api/http';

export const interaccionesService = {
  getByComercio(comercioId, { pageNumber = 1, pageSize = 10, tipoInteraccionId, desde, hasta } = {}) {
    return http.get('/api/interacciones', {
      params: { comercioId, pageNumber, pageSize, tipoInteraccionId, desde, hasta },
    });
  },

  getById(id) {
    return http.get(`/api/interacciones/${id}`);
  },

  create(payload) {
    return http.post('/api/interacciones', payload);
  },

  update(id, payload) {
    return http.put(`/api/interacciones/${id}`, payload);
  },

  remove(id) {
    return http.delete(`/api/interacciones/${id}`);
  },
};
