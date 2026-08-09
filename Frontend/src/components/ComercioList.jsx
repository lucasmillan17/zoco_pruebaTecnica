import { useCallback, useEffect, useState } from 'react';
import { api } from '../api';

const ORDENES = [
  { value: 'razonsocial', label: 'Razón social' },
  { value: 'rubro', label: 'Rubro' },
  { value: 'cuit', label: 'CUIT' },
  { value: 'estado', label: 'Estado' },
  { value: 'fechacreacion', label: 'Fecha de creación' },
  { value: 'ultimocontacto', label: 'Último contacto' },
];

const ESTADOS = ['Nuevo', 'Contactado', 'Interesado', 'Documentacion', 'Aprobado', 'Rechazado'];

function formatearFecha(iso) {
  if (!iso) return '—';
  const fecha = new Date(iso);
  if (Number.isNaN(fecha.getTime())) return iso;
  return fecha.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

export default function ComercioList({ onNuevo, onEditar, onVerInteracciones, onAnalizar }) {
  const [busqueda, setBusqueda] = useState('');
  const [estado, setEstado] = useState('');
  const [rubro, setRubro] = useState('');
  const [ordenarPor, setOrdenarPor] = useState('ultimocontacto');
  const [orden, setOrden] = useState('desc');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [datos, setDatos] = useState(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);
  const [accion, setAccion] = useState(null);

  const cargar = useCallback(async () => {
    setCargando(true);
    setError(null);
    const params = new URLSearchParams({ pageNumber: page, pageSize });
    if (busqueda.trim()) params.set('busqueda', busqueda.trim());
    if (estado) params.set('estado', estado);
    if (rubro.trim()) params.set('rubro', rubro.trim());
    if (ordenarPor) params.set('ordenarPor', ordenarPor);
    params.set('orden', orden);
    try {
      setDatos(await api.get(`/api/comercios?${params.toString()}`));
    } catch (e) {
      setError(e.message);
    } finally {
      setCargando(false);
    }
  }, [busqueda, estado, rubro, ordenarPor, orden, page, pageSize]);

  useEffect(() => {
    cargar();
  }, [cargar]);

  const cambiarFiltro = (setter) => (e) => {
    setter(e.target.value);
    setPage(1);
  };

  const toggleOrden = () => setOrden((o) => (o === 'asc' ? 'desc' : 'asc'));

  const eliminar = async (comercio) => {
    if (!window.confirm(`¿Eliminar "${comercio.razonSocial}"? (soft delete, se puede reactivar si está en Rechazado)`)) return;
    setAccion(comercio.id);
    try {
      await api.del(`/api/comercios/${comercio.id}`);
      await cargar();
    } catch (e) {
      setError(e.message);
    } finally {
      setAccion(null);
    }
  };

  const reactivar = async (comercio) => {
    if (!window.confirm(`¿Reactivar "${comercio.razonSocial}"? (Rechazado → Nuevo)`)) return;
    setAccion(comercio.id);
    try {
      await api.post(`/api/comercios/${comercio.id}/reactivar`);
      await cargar();
    } catch (e) {
      setError(e.message);
    } finally {
      setAccion(null);
    }
  };

  const items = datos?.items ?? [];
  const total = datos?.totalCount ?? 0;
  const totalPages = datos?.totalPages ?? 1;

  return (
    <div className="panel">
      <div className="toolbar">
        <input
          type="search"
          placeholder="Buscar por razón social, CUIT, contacto o email…"
          value={busqueda}
          onChange={cambiarFiltro(setBusqueda)}
        />
        <select value={estado} onChange={cambiarFiltro(setEstado)}>
          <option value="">Estado: todos</option>
          {ESTADOS.map((e) => (
            <option key={e} value={e}>
              {e}
            </option>
          ))}
        </select>
        <input
          type="text"
          placeholder="Filtrar rubro…"
          value={rubro}
          onChange={cambiarFiltro(setRubro)}
        />
        <select value={ordenarPor} onChange={cambiarFiltro(setOrdenarPor)} title="Ordenar por">
          {ORDENES.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
        <button
          type="button"
          className={`btn btn-arrow ${orden === 'asc' ? 'active' : ''}`}
          onClick={toggleOrden}
          title={orden === 'asc' ? 'Ascendente' : 'Descendente'}
        >
          {orden === 'asc' ? '↑' : '↓'}
        </button>
        <button type="button" className="btn btn-primary" onClick={onNuevo}>
          + Nuevo comercio
        </button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {cargando && <div className="alert">Cargando…</div>}

      {!cargando && items.length === 0 && !error && (
        <div className="alert">No hay comercios que coincidan. Creá uno para empezar.</div>
      )}

      {items.length > 0 && (
        <div className="tabla-wrap">
          <table className="tabla">
            <thead>
              <tr>
                <th>Razón social</th>
                <th>CUIT</th>
                <th>Rubro</th>
                <th>Contacto</th>
                <th>Teléfono</th>
                <th>Email</th>
                <th>Estado</th>
                <th>Creación</th>
                <th className="col-acciones">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {items.map((c) => (
                <tr key={c.id}>
                  <td>{c.razonSocial}</td>
                  <td>{c.cuit}</td>
                  <td>{c.rubro || '—'}</td>
                  <td>{c.nombreDelContacto || '—'}</td>
                  <td>{c.telefono || '—'}</td>
                  <td>{c.email || '—'}</td>
                  <td>
                    <span className={`badge badge-${c.estado.toLowerCase()}`}>{c.estado}</span>
                  </td>
                  <td>{formatearFecha(c.fechaDeCreacionEmpresa)}</td>
                  <td className="col-acciones">
                    <button className="btn btn-sm" onClick={() => onEditar(c)}>
                      Editar
                    </button>
                    <button className="btn btn-sm" onClick={() => onVerInteracciones(c)}>
                      Interacciones
                    </button>
                    <button className="btn btn-sm" onClick={() => onAnalizar(c)}>
                      Oportunidad
                    </button>
                    {c.estado === 'Rechazado' && (
                      <button className="btn btn-sm btn-success" onClick={() => reactivar(c)} disabled={accion === c.id}>
                        Reactivar
                      </button>
                    )}
                    <button className="btn btn-sm btn-danger" onClick={() => eliminar(c)} disabled={accion === c.id}>
                      Eliminar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="paginacion">
        <span>
          Página {datos?.pageNumber ?? 1} de {totalPages} · {total} comercios
        </span>
        <div className="paginacion-controles">
          <button className="btn btn-sm" disabled={!datos?.hasPreviousPage} onClick={() => setPage((p) => p - 1)}>
            ← Anterior
          </button>
          <select value={pageSize} onChange={(e) => { setPageSize(Number(e.target.value)); setPage(1); }}>
            <option value={10}>10 por página</option>
            <option value={25}>25 por página</option>
            <option value={50}>50 por página</option>
          </select>
          <button className="btn btn-sm" disabled={!datos?.hasNextPage} onClick={() => setPage((p) => p + 1)}>
            Siguiente →
          </button>
        </div>
      </div>
    </div>
  );
}
