import { useEffect, useState } from 'react';
import { api } from '../api';

export default function TiposTab() {
  const [tipos, setTipos] = useState([]);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    (async () => {
      setCargando(true);
      try {
        const res = await api.get('/api/tipos-interaccion');
        setTipos(res.items);
      } catch (e) {
        setError(e.message);
      } finally {
        setCargando(false);
      }
    })();
  }, []);

  return (
    <div className="panel">
      <h2>Tipos de interacción</h2>
      {cargando && <div className="alert">Cargando…</div>}
      {error && <div className="alert alert-error">{error}</div>}
      {!cargando && tipos.length === 0 && !error && <div className="alert">No hay tipos cargados.</div>}

      {tipos.length > 0 && (
        <div className="tabla-wrap">
          <table className="tabla">
            <thead>
              <tr>
                <th>Código</th>
                <th>Nombre</th>
                <th>Descripción</th>
              </tr>
            </thead>
            <tbody>
              {tipos.map((t) => (
                <tr key={t.id}>
                  <td>
                    <code>{t.codigo}</code>
                  </td>
                  <td>{t.nombre}</td>
                  <td>{t.descripcion || '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
