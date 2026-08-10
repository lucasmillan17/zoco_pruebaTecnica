import { ArrowDownAZ, ArrowUpAZ } from 'lucide-react';
import Button from '../../../shared/components/atoms/Button';
import Alert from '../../../shared/components/atoms/Alert';
import Spinner from '../../../shared/components/atoms/Spinner';
import EmptyState from '../../../shared/components/atoms/EmptyState';
import FilterBar from '../../../shared/components/organisms/FilterBar';
import SearchInput from '../../../shared/components/molecules/SearchInput';
import FilterSelect from '../../../shared/components/molecules/FilterSelect';
import Pagination from '../../../shared/components/molecules/Pagination';
import ComercioRow from '../../../shared/components/organisms/ComercioRow';
import LeyendaAcciones from '../../../shared/components/organisms/LeyendaAcciones';

const ORDENES = [
  { value: 'razonsocial', label: 'Razón social' },
  { value: 'rubro', label: 'Rubro' },
  { value: 'cuit', label: 'CUIT' },
  { value: 'estado', label: 'Estado' },
  { value: 'fechacreacion', label: 'Fecha de creación' },
  { value: 'ultimocontacto', label: 'Último contacto' },
];

const ESTADOS = ['Nuevo', 'Contactado', 'Interesado', 'Documentacion', 'Aprobado', 'Rechazado'];

export default function ComercioList(props) {
  const {
    busqueda,
    setBusqueda,
    estado,
    setEstado,
    estadoActivo,
    setEstadoActivo,
    rubro,
    setRubro,
    ordenarPor,
    setOrdenarPor,
    orden,
    toggleOrden,
    page,
    setPage,
    pageSize,
    setPageSize,
    datos,
    cargando,
    error,
    accion,
    cambiarFiltro,
    eliminar,
    reactivar,
    onNuevo,
    onEditar,
    onVerInteracciones,
    onAnalizar,
  } = props;

  const items = datos?.items ?? [];
  const total = datos?.totalCount ?? 0;
  const totalPages = datos?.totalPages ?? 1;

  return (
    <section className="rounded-md border border-gray-200 bg-white p-4 sm:p-5">
      <FilterBar>
        <SearchInput
          placeholder="Buscar por razón social, CUIT, contacto o email…"
          className="w-full sm:min-w-[240px] sm:flex-1"
          value={busqueda}
          onChange={cambiarFiltro(setBusqueda)}
        />
        <FilterSelect
          label="Estado"
          className="sm:w-40"
          value={estado}
          onChange={cambiarFiltro(setEstado)}
          options={[{ value: '', label: 'Todos' }, ...ESTADOS.map((e) => ({ value: e, label: e }))]}
        />
        <FilterSelect
          label="Visibilidad"
          className="sm:w-40"
          value={estadoActivo}
          onChange={cambiarFiltro(setEstadoActivo)}
          options={[
            { value: 'activos', label: 'Activos' },
            { value: 'inactivos', label: 'Inactivos' },
            { value: 'todos', label: 'Todos' },
          ]}
        />
        <div className="sm:w-40">
          <span className="mb-1 block text-xs font-medium text-gray-500">Rubro</span>
          <input
            type="text"
            className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text placeholder:text-muted focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary"
            placeholder="Filtrar rubro…"
            value={rubro}
            onChange={cambiarFiltro(setRubro)}
          />
        </div>
        <FilterSelect
          label="Ordenar por"
          className="sm:w-44"
          value={ordenarPor}
          onChange={cambiarFiltro(setOrdenarPor)}
          options={ORDENES.map((o) => ({ value: o.value, label: o.label }))}
        />
        <Button variant="ghost" size="md" onClick={toggleOrden} title={orden === 'asc' ? 'Ascendente' : 'Descendente'} className="self-end">
          {orden === 'asc' ? <ArrowUpAZ className="h-4 w-4" /> : <ArrowDownAZ className="h-4 w-4" />}
        </Button>
        <Button variant="primary" onClick={onNuevo} className="self-end">
          + Nuevo comercio
        </Button>
      </FilterBar>

      <div className="mt-4">
        {error && <Alert tono="error">{error}</Alert>}
        {cargando && (
          <div className="flex items-center gap-2 py-6 text-sm text-gray-500">
            <Spinner /> Cargando…
          </div>
        )}

        {!cargando && items.length === 0 && !error && (
          <EmptyState
            title="No hay comercios que coincidan"
            description="Creá uno para empezar."
          >
            <Button variant="primary" onClick={onNuevo}>
              + Nuevo comercio
            </Button>
          </EmptyState>
        )}

        {items.length > 0 && (
          <>
            <LeyendaAcciones />
            <div className="mt-2 overflow-x-auto rounded-md border border-gray-200">
              <table className="w-full min-w-[900px] border-collapse text-left">
                <thead>
                  <tr className="border-b border-gray-200 bg-gray-50 text-xs font-semibold uppercase tracking-wide text-gray-500">
                    <th className="px-3 py-2.5">Razón social</th>
                    <th className="px-3 py-2.5">CUIT</th>
                    <th className="px-3 py-2.5">Rubro</th>
                    <th className="px-3 py-2.5">Contacto</th>
                    <th className="px-3 py-2.5">Teléfono</th>
                    <th className="px-3 py-2.5">Email</th>
                    <th className="px-3 py-2.5">Creación</th>
                    <th className="px-3 py-2.5">Estado</th>
                    <th className="px-3 py-2.5 text-right">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((c) => (
                    <ComercioRow
                      key={c.id}
                      comercio={c}
                      accion={accion}
                      onEditar={onEditar}
                      onVerInteracciones={onVerInteracciones}
                      onAnalizar={onAnalizar}
                      onReactivar={reactivar}
                      onEliminar={eliminar}
                    />
                  ))}
                </tbody>
              </table>
            </div>
          </>
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
          />
        )}
      </div>
    </section>
  );
}
