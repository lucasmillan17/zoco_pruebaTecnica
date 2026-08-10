import { useEffect, useState } from 'react';
import Alert from '../../../shared/components/atoms/Alert';
import Badge from '../../../shared/components/atoms/Badge';
import EmptyState from '../../../shared/components/atoms/EmptyState';
import Input from '../../../shared/components/atoms/Input';
import Spinner from '../../../shared/components/atoms/Spinner';
import FilterSelect from '../../../shared/components/molecules/FilterSelect';
import Pagination from '../../../shared/components/molecules/Pagination';
import { useDebounce } from '../../../shared/hooks/useDebounce';
import { auditoriaService } from '../services/auditoriaService';
import { formatearFecha } from '../../../shared/utils/format';

const ENTIDADES = [
  { value: 'Comercio', label: 'Comercio' },
  { value: 'Interaccion', label: 'Interacción' },
  { value: 'TipoInteraccion', label: 'Tipo de interacción' },
  { value: 'Usuario', label: 'Usuario' },
  { value: '', label: 'Todas' },
];

const OPERACIONES = [
  { value: '', label: 'Todas' },
  { value: 'Crear', label: 'Crear' },
  { value: 'Actualizar', label: 'Actualizar' },
  { value: 'Eliminar', label: 'Eliminar' },
];

const OPERACION_VARIANTE = { Crear: 'green', Actualizar: 'blue', Eliminar: 'red' };

export default function AuditoriaPage() {
  const [entidad, setEntidad] = useState('Comercio');
  const [operacion, setOperacion] = useState('');
  const [usuario, setUsuario] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [datos, setDatos] = useState(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);

  const usuarioDeb = useDebounce(usuario.trim(), 400);

  const cambiarFiltro = (setter) => (e) => {
    setter(e.target.value);
    setPage(1);
  };

  useEffect(() => {
    let activo = true;

    (async () => {
      setCargando(true);
      setError(null);
      const params = { pageNumber: page, pageSize, entidad: entidad || undefined };
      if (usuarioDeb) params.usuario = usuarioDeb;
      if (operacion) params.operacion = operacion;
      if (desde) params.desde = `${desde}T00:00:00.000Z`;
      if (hasta) params.hasta = `${hasta}T23:59:59.999Z`;

      try {
        const res = await auditoriaService.getAll(params);
        if (activo) setDatos(res);
      } catch (e) {
        if (activo) setError(e.message);
      } finally {
        if (activo) setCargando(false);
      }
    })();

    return () => {
      activo = false;
    };
  }, [entidad, operacion, usuarioDeb, desde, hasta, page, pageSize]);

  const items = datos?.items ?? [];
  const total = datos?.totalCount ?? 0;
  const totalPages = datos?.totalPages ?? 1;

  return (
    <div className="mx-auto max-w-7xl">
      <header className="mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Auditoría</h1>
        <p className="mt-1 text-sm text-muted">
          Registro automático de los cambios realizados sobre los datos del sistema.
        </p>
      </header>

      <div className="mb-4 flex flex-wrap items-end gap-3">
        <FilterSelect
          label="Entidad"
          className="sm:w-44"
          value={entidad}
          onChange={cambiarFiltro(setEntidad)}
          options={ENTIDADES}
        />
        <FilterSelect
          label="Operación"
          className="sm:w-40"
          value={operacion}
          onChange={cambiarFiltro(setOperacion)}
          options={OPERACIONES}
        />
        <div className="w-full sm:w-48">
          <span className="mb-1 block text-xs font-medium text-gray-500">Usuario</span>
          <Input value={usuario} onChange={cambiarFiltro(setUsuario)} placeholder="Filtrar por usuario…" />
        </div>
        <div className="w-full sm:w-44">
          <span className="mb-1 block text-xs font-medium text-gray-500">Desde</span>
          <Input type="date" value={desde} onChange={cambiarFiltro(setDesde)} />
        </div>
        <div className="w-full sm:w-44">
          <span className="mb-1 block text-xs font-medium text-gray-500">Hasta</span>
          <Input type="date" value={hasta} onChange={cambiarFiltro(setHasta)} />
        </div>
      </div>

      <section className="rounded-md border border-gray-200 bg-white p-4 sm:p-5">
        {cargando && (
          <div className="flex items-center gap-2 py-6 text-sm text-gray-500">
            <Spinner /> Cargando…
          </div>
        )}
        {error && <Alert tono="error">{error}</Alert>}

        {!cargando && items.length === 0 && !error && (
          <EmptyState title="No hay registros de auditoría" description="Probá cambiar los filtros de búsqueda." />
        )}

        {items.length > 0 && (
          <div className="overflow-x-auto rounded-md border border-gray-200">
            <table className="w-full min-w-[860px] border-collapse text-left">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50 text-xs font-semibold uppercase tracking-wide text-gray-500">
                  <th className="px-3 py-2.5">Fecha</th>
                  <th className="px-3 py-2.5">Usuario</th>
                  <th className="px-3 py-2.5">Rol</th>
                  <th className="px-3 py-2.5">Operación</th>
                  <th className="px-3 py-2.5">Campo</th>
                  <th className="px-3 py-2.5">Valor anterior</th>
                  <th className="px-3 py-2.5">Valor nuevo</th>
                </tr>
              </thead>
              <tbody>
                {items.map((registro) => (
                  <tr key={registro.id} className="border-b border-gray-200 hover:bg-gray-50">
                    <td className="whitespace-nowrap px-3 py-2.5 text-sm text-gray-600">
                      {formatearFecha(registro.fecha, { conHora: true })}
                    </td>
                    <td className="px-3 py-2.5">
                      <code className="rounded bg-gray-100 px-2 py-0.5 text-xs text-gray-700">
                        {registro.usuario || '—'}
                      </code>
                    </td>
                    <td className="px-3 py-2.5 text-sm text-gray-600">{registro.rol || '—'}</td>
                    <td className="px-3 py-2.5">
                      <Badge variant={OPERACION_VARIANTE[registro.operacion] ?? 'gray'} tone="soft">
                        {registro.operacion}
                      </Badge>
                    </td>
                    <td className="px-3 py-2.5">
                      <code className="rounded bg-gray-100 px-2 py-0.5 text-xs text-gray-700">
                        {registro.campo}
                      </code>
                    </td>
                    <td className="max-w-[220px] truncate px-3 py-2.5 text-sm text-gray-600" title={registro.valorAnterior || ''}>
                      {registro.valorAnterior || '—'}
                    </td>
                    <td className="max-w-[220px] truncate px-3 py-2.5 text-sm text-gray-800" title={registro.valorNuevo || ''}>
                      {registro.valorNuevo || '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {items.length > 0 && (
          <Pagination
            page={page}
            pageSize={pageSize}
            total={total}
            totalPages={totalPages}
            hasPreviousPage={datos?.hasPreviousPage}
            hasNextPage={datos?.hasNextPage}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
            etiqueta="registros"
          />
        )}
      </section>
    </div>
  );
}
