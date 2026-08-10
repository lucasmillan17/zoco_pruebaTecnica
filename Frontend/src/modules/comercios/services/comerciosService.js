import http from '../../../shared/api/http';

export const comerciosService = {
  getAll(params) {
    return http.get('/api/comercios', { params });
  },

  getById(id) {
    return http.get(`/api/comercios/${id}`);
  },

  create(payload) {
    return http.post('/api/comercios', payload);
  },

  update(id, payload) {
    return http.put(`/api/comercios/${id}`, payload);
  },

  remove(id) {
    return http.delete(`/api/comercios/${id}`);
  },

  reactivar(id) {
    return http.post(`/api/comercios/${id}/reactivar`);
  },

  analizarOportunidad(id) {
    return http.post(`/api/comercios/${id}/oportunidad`);
  },

  validarCuit(cuit) {
    return http.get('/api/comercios/validar-cuit', { params: { cuit } });
  },
};
