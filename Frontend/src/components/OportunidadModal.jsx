import { useEffect, useState } from 'react';
import { api } from '../api';
import Modal from './Modal';

export default function OportunidadModal({ comercio, onClose }) {
  const [resultado, setResultado] = useState(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);

  const analizar = async () => {
    setCargando(true);
    setError(null);
    try {
      setResultado(await api.post(`/api/comercios/${comercio.id}/oportunidad`));
    } catch (e) {
      setError(e.message);
    } finally {
      setCargando(false);
    }
  };

  useEffect(() => {
    analizar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const nivel = resultado?.nivelInteres || '';

  return (
    <Modal title={`Análisis de oportunidad: ${comercio.razonSocial}`} onClose={onClose}>
      {cargando && <div className="alert">Analizando con IA…</div>}
      {error && <div className="alert alert-error">{error}</div>}

      {resultado && (
        <div className="oportunidad">
          <p>
            <strong>Nivel de interés:</strong>{' '}
            <span className={`badge badge-nivel-${nivel}`}>{nivel}</span>
          </p>
          <p>
            <strong>Resumen:</strong> {resultado.resumen}
          </p>
          {resultado.proximoPaso && (
            <p>
              <strong>Próximo paso:</strong> {resultado.proximoPaso}
            </p>
          )}

          {resultado.preguntas?.length > 0 && (
            <div>
              <strong>Preguntas para hacer:</strong>
              <ul>
                {resultado.preguntas.map((p, idx) => (
                  <li key={idx}>{p}</li>
                ))}
              </ul>
            </div>
          )}

          {resultado.datosFaltantes?.length > 0 && (
            <div>
              <strong>Datos faltantes:</strong>
              <ul>
                {resultado.datosFaltantes.map((d, idx) => (
                  <li key={idx}>{d}</li>
                ))}
              </ul>
            </div>
          )}

          {!resultado.preguntas?.length && !resultado.datosFaltantes?.length && (
            <p className="muted">Sin preguntas ni datos faltantes para este comercio.</p>
          )}
        </div>
      )}
    </Modal>
  );
}
