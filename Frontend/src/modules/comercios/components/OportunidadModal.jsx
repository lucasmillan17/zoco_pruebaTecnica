import { useEffect, useState } from 'react';
import Modal from '../../../shared/components/atoms/Modal';
import Badge from '../../../shared/components/atoms/Badge';
import Alert from '../../../shared/components/atoms/Alert';
import Spinner from '../../../shared/components/atoms/Spinner';
import { comerciosService } from '../services/comerciosService';

const VARIANTE_NIVEL = { alto: 'green', medio: 'amber', bajo: 'gray' };

export default function OportunidadModal({ comercio, onClose }) {
  const [resultado, setResultado] = useState(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);

  const analizar = async () => {
    setCargando(true);
    setError(null);
    try {
      setResultado(await comerciosService.analizarOportunidad(comercio.id));
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
      {cargando && (
        <div className="flex items-center gap-2 text-sm text-gray-500">
          <Spinner /> Analizando con IA…
        </div>
      )}
      {error && <Alert tono="error">{error}</Alert>}

      {resultado && (
        <div className="space-y-3 text-sm">
          <p>
            <strong className="text-gray-900">Nivel de interés:</strong>{' '}
            <Badge variant={VARIANTE_NIVEL[nivel] ?? 'gray'}>{nivel}</Badge>
          </p>
          <p className="text-gray-700">
            <strong className="text-gray-900">Resumen:</strong> {resultado.resumen}
          </p>
          {resultado.proximoPaso && (
            <p className="text-gray-700">
              <strong className="text-gray-900">Próximo paso:</strong> {resultado.proximoPaso}
            </p>
          )}

          {resultado.preguntas?.length > 0 && (
            <div>
              <strong className="text-gray-900">Preguntas para hacer:</strong>
              <ul className="mt-1 list-disc pl-5 text-gray-700">
                {resultado.preguntas.map((p, idx) => (
                  <li key={idx}>{p}</li>
                ))}
              </ul>
            </div>
          )}

          {resultado.datosFaltantes?.length > 0 && (
            <div>
              <strong className="text-gray-900">Datos faltantes:</strong>
              <ul className="mt-1 list-disc pl-5 text-gray-700">
                {resultado.datosFaltantes.map((d, idx) => (
                  <li key={idx}>{d}</li>
                ))}
              </ul>
            </div>
          )}

          {!resultado.preguntas?.length && !resultado.datosFaltantes?.length && (
            <p className="text-gray-500">Sin preguntas ni datos faltantes para este comercio.</p>
          )}
        </div>
      )}
    </Modal>
  );
}
