import { useCallback, useEffect, useState } from 'react';
import { useDebounce } from '../../../shared/hooks/useDebounce';
import { useToast } from '../../../shared/context/ToastProvider';
import { comerciosService } from '../services/comerciosService';

export function useComercios() {
  const toast = useToast();
  const [busqueda, setBusqueda] = useState('');
  const [estado, setEstado] = useState('');
  const [estadoActivo, setEstadoActivo] = useState('activos');
  const [rubro, setRubro] = useState('');
  const [ordenarPor, setOrdenarPor] = useState('ultimocontacto');
  const [orden, setOrden] = useState('desc');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [datos, setDatos] = useState(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);
  const [accion, setAccion] = useState(null);

  const busquedaDeb = useDebounce(busqueda.trim(), 400);
  const rubroDeb = useDebounce(rubro.trim(), 400);

  const cargar = useCallback(async () => {
    setCargando(true);
    setError(null);
    const params = { pageNumber: page, pageSize, orden, estadoActivo };
    if (busquedaDeb) params.busqueda = busquedaDeb;
    if (rubroDeb) params.rubro = rubroDeb;
    if (estado) params.estado = estado;
    if (ordenarPor) params.ordenarPor = ordenarPor;
    try {
      setDatos(await comerciosService.getAll(params));
    } catch (e) {
      setError(e.message);
    } finally {
      setCargando(false);
    }
  }, [busquedaDeb, estado, estadoActivo, rubroDeb, ordenarPor, orden, page, pageSize]);

  useEffect(() => {
    cargar();
  }, [cargar]);

  const cambiarFiltro = (setter) => (e) => {
    setter(e.target.value);
    setPage(1);
  };

  const toggleOrden = () => setOrden((o) => (o === 'asc' ? 'desc' : 'asc'));

  const eliminar = async (comercio) => {
    if (!window.confirm(`¿Eliminar "${comercio.razonSocial}"? (soft delete, se puede reactivar después)`)) return;
    setAccion(comercio.id);
    try {
      await comerciosService.remove(comercio.id);
      await cargar();
      toast.success(`"${comercio.razonSocial}" eliminado (se puede reactivar).`);
    } catch (e) {
      setError(e.message);
    } finally {
      setAccion(null);
    }
  };

  const reactivar = async (comercio) => {
    if (!window.confirm(`¿Reactivar "${comercio.razonSocial}"? (vuelve a Nuevo)`)) return;
    setAccion(comercio.id);
    try {
      await comerciosService.reactivar(comercio.id);
      await cargar();
      toast.success(`"${comercio.razonSocial}" reactivado.`);
    } catch (e) {
      setError(e.message);
    } finally {
      setAccion(null);
    }
  };

  return {
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
    cargar,
  };
}
