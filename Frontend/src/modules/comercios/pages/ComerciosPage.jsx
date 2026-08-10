import { useState } from 'react';
import { CircleHelp } from 'lucide-react';
import ComercioList from '../components/ComercioList';
import ComercioForm from '../components/ComercioForm';
import OportunidadModal from '../components/OportunidadModal';
import InteraccionesModal from '../../interacciones/components/InteraccionesModal';
import { useComercios } from '../hooks/useComercios';

export default function ComerciosPage() {
  const comercios = useComercios();
  const [creando, setCreando] = useState(false);
  const [editando, setEditando] = useState(null);
  const [interacciones, setInteracciones] = useState(null);
  const [oportunidad, setOportunidad] = useState(null);

  const refrescar = () => comercios.cargar();

  return (
    <div className="mx-auto max-w-7xl">
      <header className="mb-6 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Comercios</h1>
        <a
          href="#ayuda"
          className="inline-flex items-center gap-1.5 text-sm text-primary hover:text-primary-hover"
        >
          <CircleHelp className="h-4 w-4" /> Cómo gestionar tus comercios
        </a>
      </header>

      <ComercioList
        {...comercios}
        onNuevo={() => setCreando(true)}
        onEditar={(c) => setEditando(c)}
        onVerInteracciones={(c) => setInteracciones(c)}
        onAnalizar={(c) => setOportunidad(c)}
      />

      {creando && (
        <ComercioForm
          onClose={() => setCreando(false)}
          onGuardado={() => {
            setCreando(false);
            refrescar();
          }}
        />
      )}

      {editando && (
        <ComercioForm
          comercio={editando}
          onClose={() => setEditando(null)}
          onGuardado={() => {
            setEditando(null);
            refrescar();
          }}
        />
      )}

      {interacciones && <InteraccionesModal comercio={interacciones} onClose={() => setInteracciones(null)} />}

      {oportunidad && <OportunidadModal comercio={oportunidad} onClose={() => setOportunidad(null)} />}
    </div>
  );
}
