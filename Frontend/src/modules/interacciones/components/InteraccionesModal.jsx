import { useState } from 'react';
import Modal from '../../../shared/components/atoms/Modal';
import Button from '../../../shared/components/atoms/Button';
import Alert from '../../../shared/components/atoms/Alert';
import Spinner from '../../../shared/components/atoms/Spinner';
import Input from '../../../shared/components/atoms/Input';
import FormField from '../../../shared/components/molecules/FormField';
import FilterSelect from '../../../shared/components/molecules/FilterSelect';
import Pagination from '../../../shared/components/molecules/Pagination';
import Badge from '../../../shared/components/atoms/Badge';
import EmptyState from '../../../shared/components/atoms/EmptyState';
import { useToast } from '../../../shared/context/ToastProvider';
import { useInteracciones } from '../hooks/useInteracciones';
import { formatearFecha } from '../../../shared/utils/format';

export default function InteraccionesModal({ comercio, onClose }) {
  const toast = useToast();
  const {
    tipos,
    lista,
    datos,
    cargando,
    error,
    tipoSeleccionado,
    setTipoSeleccionado,
    filtroTipo,
    desde,
    hasta,
    page,
    pageSize,
    cambiarFiltro,
    setFiltroTipo,
    setDesde,
    setHasta,
    setPage,
    cambiarPageSize,
    agregar,
  } = useInteracciones(comercio.id);

  const [formAbierto, setFormAbierto] = useState(false);
  const [fechaInteraccion, setFechaInteraccion] = useState('');
  const [notas, setNotas] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [errorForm, setErrorForm] = useState(null);

  const total = datos?.totalCount ?? 0;
  const totalPages = datos?.totalPages ?? 1;

  const guardar = async (e) => {
    e.preventDefault();
    setEnviando(true);
    setErrorForm(null);
    const fechaIso = fechaInteraccion ? new Date(fechaInteraccion).toISOString() : null;
    try {
      await agregar(fechaIso, notas.trim() || null);
      toast.success('Interacción registrada.');
      setNotas('');
      setFechaInteraccion('');
    } catch (err) {
      setErrorForm(err.message);
    } finally {
      setEnviando(false);
    }
  };

  return (
    <Modal title={`Interacciones: ${comercio.razonSocial}`} onClose={onClose}>
      {cargando && (
        <div className="flex items-center gap-2 text-sm text-gray-500">
          <Spinner /> Cargando…
        </div>
      )}
      {(error || errorForm) && <Alert tono="error">{error || errorForm}</Alert>}

      <div className="flex flex-wrap items-end gap-4 border-b border-gray-200 pb-4">
        <FilterSelect
          label="Filtrar tipo"
          className="sm:w-52"
          value={filtroTipo}
          onChange={cambiarFiltro(setFiltroTipo)}
          options={[{ value: '', label: 'Todos los tipos' }, ...tipos.map((t) => ({ value: t.id, label: t.nombre }))]}
        />
        <FormField label="Desde" className="sm:w-44">
          <Input type="date" value={desde} onChange={cambiarFiltro(setDesde)} />
        </FormField>
        <FormField label="Hasta" className="sm:w-44">
          <Input type="date" value={hasta} onChange={cambiarFiltro(setHasta)} />
        </FormField>
        <Button
          variant="primary"
          onClick={() => setFormAbierto((v) => !v)}
          className="ml-auto self-end"
          disabled={tipos.length === 0}
          title={tipos.length === 0 ? 'Creá primero un tipo de interacción' : undefined}
        >
          {formAbierto ? 'Ocultar formulario' : '+ Registrar interacción'}
        </Button>
      </div>

      {formAbierto && (
        <form
          onSubmit={guardar}
          className="mt-4 flex flex-col gap-4 rounded-md border border-gray-200 bg-gray-50 p-4 sm:flex-row sm:items-end"
        >
          <FormField label="Tipo" required className="sm:w-48">
            <select
              value={tipoSeleccionado}
              onChange={(e) => setTipoSeleccionado(e.target.value)}
              required
              className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary"
            >
              {tipos.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.nombre}
                </option>
              ))}
            </select>
          </FormField>

          <FormField label="Fecha" className="sm:w-52">
            <Input type="datetime-local" value={fechaInteraccion} onChange={(e) => setFechaInteraccion(e.target.value)} />
          </FormField>

          <FormField label="Notas" className="grow">
            <Input value={notas} onChange={(e) => setNotas(e.target.value)} maxLength={2000} />
          </FormField>

          <Button type="submit" variant="primary" disabled={enviando || cargando} className="self-end">
            {enviando ? 'Registrando…' : 'Registrar'}
          </Button>
        </form>
      )}

      <div className="mt-4">
        {!cargando && lista.length === 0 && (
          <EmptyState title="No hay interacciones que coincidan" description="Probá cambiar los filtros o registrar una nueva." />
        )}

        {lista.length > 0 && (
          <ul className="divide-y divide-gray-200">
            {lista.map((i) => (
              <li key={i.id} className="flex flex-col gap-1 py-3 sm:flex-row sm:items-center sm:gap-4">
                <span className="w-36 shrink-0 text-sm text-gray-500">
                  {formatearFecha(i.fechaInteraccion, { conHora: true })}
                </span>
                <span className="w-32 shrink-0">
                  <Badge variant="gray" tone="soft">{i.tipoNombre || '—'}</Badge>
                </span>
                <span className="text-sm text-gray-700">{i.notas || '—'}</span>
              </li>
            ))}
          </ul>
        )}

        {!cargando && lista.length > 0 && (
          <Pagination
            page={page}
            pageSize={pageSize}
            total={total}
            totalPages={totalPages}
            hasPreviousPage={datos?.hasPreviousPage}
            hasNextPage={datos?.hasNextPage}
            onPageChange={setPage}
            onPageSizeChange={cambiarPageSize}
            etiqueta="interacciones"
          />
        )}
      </div>
    </Modal>
  );
}
