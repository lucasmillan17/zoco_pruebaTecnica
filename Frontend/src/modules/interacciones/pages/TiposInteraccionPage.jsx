import { useEffect, useState } from 'react';
import { ChevronDown, ChevronRight, Pencil, Plus, RotateCcw, Trash2 } from 'lucide-react';
import Alert from '../../../shared/components/atoms/Alert';
import Spinner from '../../../shared/components/atoms/Spinner';
import EmptyState from '../../../shared/components/atoms/EmptyState';
import Button from '../../../shared/components/atoms/Button';
import IconButton from '../../../shared/components/atoms/IconButton';
import Badge from '../../../shared/components/atoms/Badge';
import { tiposInteraccionService } from '../services/tiposInteraccionService';
import TipoInteraccionModal from '../components/TipoInteraccionModal';
import FilterSelect from '../../../shared/components/molecules/FilterSelect';
import { useToast } from '../../../shared/context/ToastProvider';

function FilaTipo({ tipo, onEditar, onEliminar, onReactivar, inactivo = false }) {
  return (
    <tr className={`border-b border-gray-200 ${inactivo ? 'bg-gray-50' : ''}`}>
      <td className="px-3 py-2.5">
        <code className="rounded bg-gray-100 px-2 py-0.5 text-xs text-gray-700">{tipo.codigo}</code>
      </td>
      <td className="px-3 py-2.5 text-sm font-medium text-gray-900">{tipo.nombre}</td>
      <td className="px-3 py-2.5 text-sm text-gray-600">{tipo.descripcion || '—'}</td>
      <td className="px-3 py-2.5">
        <div className="flex items-center justify-end gap-0.5">
          <IconButton titulo="Editar" onClick={() => onEditar(tipo)}>
            <Pencil className="h-4 w-4" />
          </IconButton>
          {inactivo ? (
            onReactivar && (
              <IconButton titulo="Reactivar" onClick={() => onReactivar(tipo)}>
                <RotateCcw className="h-4 w-4" />
              </IconButton>
            )
          ) : (
            onEliminar && (
              <IconButton titulo="Eliminar" onClick={() => onEliminar(tipo)} className="hover:text-danger">
                <Trash2 className="h-4 w-4" />
              </IconButton>
            )
          )}
        </div>
      </td>
    </tr>
  );
}

export default function TiposInteraccionPage() {
  const toast = useToast();
  const [tipos, setTipos] = useState([]);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);
  const [modalAbierto, setModalAbierto] = useState(false);
  const [editando, setEditando] = useState(null);
  const [estadoActivo, setEstadoActivo] = useState('activos');
  const [mostrarInactivos, setMostrarInactivos] = useState(false);

  const cargar = async () => {
    setCargando(true);
    setError(null);
    try {
      const res = await tiposInteraccionService.getAll({ pageSize: 100, estadoActivo });
      setTipos(res.items);
    } catch (e) {
      setError(e.message);
    } finally {
      setCargando(false);
    }
  };

  useEffect(() => {
    cargar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [estadoActivo]);

  const cambiarEstadoActivo = (valor) => {
    setEstadoActivo(valor);
    setMostrarInactivos(false);
  };

  const activos = tipos.filter((t) => t.activo);
  const inactivos = tipos.filter((t) => !t.activo);

  const abrirNuevo = () => {
    setEditando(null);
    setModalAbierto(true);
  };

  const abrirEdicion = (tipo) => {
    setEditando(tipo);
    setModalAbierto(true);
  };

  const eliminar = async (tipo) => {
    if (!window.confirm(`¿Desactivar el tipo "${tipo.nombre}"?`)) return;
    setError(null);
    try {
      await tiposInteraccionService.remove(tipo.id);
      await cargar();
      toast.success(`Tipo "${tipo.nombre}" desactivado.`);
    } catch (e) {
      setError(e.message);
    }
  };

  const reactivar = async (tipo) => {
    setError(null);
    try {
      await tiposInteraccionService.reactivar(tipo.id);
      await cargar();
      toast.success(`Tipo "${tipo.nombre}" reactivado.`);
    } catch (e) {
      setError(e.message);
    }
  };

  return (
    <div className="mx-auto max-w-5xl">
      <header className="mb-6 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Tipos de interacción</h1>
        <Button variant="primary" onClick={abrirNuevo}>
          <Plus className="h-4 w-4" /> Nuevo tipo
        </Button>
      </header>

      <div className="mb-4">
        <FilterSelect
          label="Visibilidad"
          className="sm:w-40"
          value={estadoActivo}
          onChange={(e) => cambiarEstadoActivo(e.target.value)}
          options={[
            { value: 'activos', label: 'Activos' },
            { value: 'inactivos', label: 'Inactivos' },
            { value: 'todos', label: 'Todos' },
          ]}
        />
      </div>

      <section className="rounded-md border border-gray-200 bg-white p-4 sm:p-5">
        {cargando && (
          <div className="flex items-center gap-2 py-6 text-sm text-gray-500">
            <Spinner /> Cargando…
          </div>
        )}
        {error && <Alert tono="error">{error}</Alert>}

        {!cargando && tipos.length === 0 && !error && (
          <EmptyState title="No hay tipos cargados." description="Creá el primero para poder registrar interacciones.">
            <Button variant="primary" onClick={abrirNuevo}>
              <Plus className="h-4 w-4" /> Nuevo tipo
            </Button>
          </EmptyState>
        )}

        {!cargando && estadoActivo === 'todos' && activos.length > 0 && (
          <div className="overflow-x-auto rounded-md border border-gray-200">
            <table className="w-full min-w-[640px] border-collapse text-left">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50 text-xs font-semibold uppercase tracking-wide text-gray-500">
                  <th className="px-3 py-2.5">Código</th>
                  <th className="px-3 py-2.5">Nombre</th>
                  <th className="px-3 py-2.5">Descripción</th>
                  <th className="px-3 py-2.5 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody>
                {activos.map((t) => (
                  <FilaTipo key={t.id} tipo={t} onEditar={abrirEdicion} onEliminar={eliminar} />
                ))}
              </tbody>
            </table>
          </div>
        )}

        {!cargando && estadoActivo !== 'todos' && tipos.length > 0 && (
          <div className="overflow-x-auto rounded-md border border-gray-200">
            <table className="w-full min-w-[640px] border-collapse text-left">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50 text-xs font-semibold uppercase tracking-wide text-gray-500">
                  <th className="px-3 py-2.5">Código</th>
                  <th className="px-3 py-2.5">Nombre</th>
                  <th className="px-3 py-2.5">Descripción</th>
                  <th className="px-3 py-2.5 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody>
                {tipos.map((t) => (
                  <FilaTipo
                    key={t.id}
                    tipo={t}
                    inactivo={!t.activo}
                    onEditar={abrirEdicion}
                    onReactivar={reactivar}
                    onEliminar={eliminar}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}

        {!cargando && estadoActivo === 'todos' && inactivos.length > 0 && (
          <div className="mt-5">
            <button
              type="button"
              onClick={() => setMostrarInactivos((v) => !v)}
              className="inline-flex items-center gap-1.5 text-sm font-medium text-gray-700 hover:text-gray-900"
            >
              {mostrarInactivos ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
              Inactivos
              <Badge variant="gray" tone="soft">{inactivos.length}</Badge>
            </button>

            {mostrarInactivos && (
              <div className="mt-2 overflow-x-auto rounded-md border border-gray-200">
                <table className="w-full min-w-[640px] border-collapse text-left">
                  <thead>
                    <tr className="border-b border-gray-200 bg-gray-50 text-xs font-semibold uppercase tracking-wide text-gray-500">
                      <th className="px-3 py-2.5">Código</th>
                      <th className="px-3 py-2.5">Nombre</th>
                      <th className="px-3 py-2.5">Descripción</th>
                      <th className="px-3 py-2.5 text-right">Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    {inactivos.map((t) => (
                      <FilaTipo key={t.id} tipo={t} onEditar={abrirEdicion} onReactivar={reactivar} inactivo />
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </section>

      {modalAbierto && (
        <TipoInteraccionModal tipo={editando} onGuardado={cargar} onClose={() => setModalAbierto(false)} />
      )}
    </div>
  );
}
