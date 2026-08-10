import http from '../../../shared/api/http';

export const auditoriaService = {
  getAll(params) {
    return http.get('/api/auditoria', { params });
  },
};
