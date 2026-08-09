import { useState } from 'react';
import ComercioList from './components/ComercioList.jsx';
import ComercioForm from './components/ComercioForm.jsx';
import InteraccionesModal from './components/InteraccionesModal.jsx';
import OportunidadModal from './components/OportunidadModal.jsx';
import TiposTab from './components/TiposTab.jsx';

export default function App() {
  const [tab, setTab] = useState('comercios');
  const [creando, setCreando] = useState(false);
  const [editando, setEditando] = useState(null);
  const [interacciones, setInteracciones] = useState(null);
  const [oportunidad, setOportunidad] = useState(null);
  const [reloadKey, setReloadKey] = useState(0);

  const refrescar = () => setReloadKey((k) => k + 1);

  return (
    <div className="app">
      <header className="app-header">
        <h1>CMS Zoco — Gestor de Comercios</h1>
        <nav className="tabs">
          <button
            type="button"
            className={`tab ${tab === 'comercios' ? 'active' : ''}`}
            onClick={() => setTab('comercios')}
          >
            Comercios
          </button>
          <button
            type="button"
            className={`tab ${tab === 'tipos' ? 'active' : ''}`}
            onClick={() => setTab('tipos')}
          >
            Tipos de interacción
          </button>
        </nav>
      </header>

      <main>
        {tab === 'comercios' && (
          <ComercioList
            key={reloadKey}
            onNuevo={() => setCreando(true)}
            onEditar={(c) => setEditando(c)}
            onVerInteracciones={(c) => setInteracciones(c)}
            onAnalizar={(c) => setOportunidad(c)}
          />
        )}
        {tab === 'tipos' && <TiposTab />}
      </main>

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
