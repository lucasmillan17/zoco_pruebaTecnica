import { useCallback, useEffect, useState } from 'react';
import { api } from '../api';
import Modal from './Modal';

function formatearFecha(iso) {
  if (!iso) return 'Sin fecha';
  const fecha = new Date(iso);
  if (Number.isNaN(fecha.getTime())) return iso;
  return fecha.toLocaleString('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export default function InteraccionesModal({ comercio, onClose }) {
  const [tipos, setTipos] = useState([]);
  const [lista, setLista] = useState([]);
  const [form, setForm] = useState({ tipoInteraccionId: '', fechaInteraccion: '', notas: '' });
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);

  const cargar = useCallback(async () => {
    setCargando(true);
    setError(null);
    try {
      const [tiposRes, listaRes] = await Promise.all([
        api.get('/api/tipos-interaccion'),
        api.get(`/api/interacciones?comercioId=${comercio.id}&pageSize=100`),
      ]);
      setTipos(tiposRes.items);
      setLista(listaRes.items);
      if (!form.tipoInteraccionId && tiposRes.items.length > 0) {
        setForm((f) => ({ ...f, tipoInteraccionId: tiposRes.items[0].id }));
      }
    } catch (e) {
      setError(e.message);
    } finally {
      setCargando(false);
    }
  }, [comercio.id]);

  useEffect(() => {
    cargar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [cargar]);

  const set = (campo) => (e) => setForm((f) => ({ ...f, [campo]: e.target.value }));

  const agregar = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await api.post('/api/interacciones', {
        comercioId: comercio.id,
        tipoInteraccionId: form.tipoInteraccionId,
        fechaInteraccion: form.fechaInteraccion || null,
        notas: form.notas.trim() || null,
      });
      setForm((f) => ({ ...f, notas: '', fechaInteraccion: '' }));
      const listaRes = await api.get(`/api/interacciones?comercioId=${comercio.id}&pageSize=100`);
      setLista(listaRes.items);
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <Modal title={`Interacciones: ${comercio.razonSocial}`} onClose={onClose}>
      {cargando && <div className="alert">Cargando…</div>}
      {error && <div className="alert alert-error">{error}</div>}

      <form onSubmit={agregar} className="form form-row">
        <label>
          Tipo *
          <select value={form.tipoInteraccionId} onChange={set('tipoInteraccionId')} required>
            {tipos.map((t) => (
              <option key={t.id} value={t.id}>
                {t.nombre}
              </option>
            ))}
          </select>
        </label>
        <label>
          Fecha
          <input type="datetime-local" value={form.fechaInteraccion} onChange={set('fechaInteraccion')} />
        </label>
        <label className="grow">
          Notas
          <input value={form.notas} onChange={set('notas')} maxLength={2000} />
        </label>
        <button type="submit" className="btn btn-primary align-end">
          Registrar
        </button>
      </form>

      {lista.length === 0 && !cargando && <div className="alert">Todavía no hay interacciones registradas.</div>}

      {lista.length > 0 && (
        <div className="tabla-wrap">
          <table className="tabla">
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Tipo</th>
                <th>Notas</th>
              </tr>
            </thead>
            <tbody>
              {lista.map((i) => (
                <tr key={i.id}>
                  <td>{formatearFecha(i.fechaInteraccion)}</td>
                  <td>
                    <span className="badge badge-tipo">{i.tipoNombre || '—'}</span>
                  </td>
                  <td>{i.notas || '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Modal>
  );
}
