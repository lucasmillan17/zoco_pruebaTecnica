import { useCallback, useEffect, useState } from 'react';
import { interaccionesService } from '../services/interaccionesService';
import { tiposInteraccionService } from '../services/tiposInteraccionService';
import { useDebounce } from '../../../shared/hooks/useDebounce';

export function useInteracciones(comercioId) {
  const [tipos, setTipos] = useState([]);
  const [tipoSeleccionado, setTipoSeleccionado] = useState('');
  const [lista, setLista] = useState([]);
  const [datos, setDatos] = useState(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);

  const [filtroTipo, setFiltroTipo] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const filtroTipoDeb = useDebounce(filtroTipo, 300);
  const desdeDeb = useDebounce(desde, 400);
  const hastaDeb = useDebounce(hasta, 400);

  const paramsDe = useCallback(
    (pag) => ({
      pageNumber: pag,
      pageSize,
      tipoInteraccionId: filtroTipoDeb || undefined,
      desde: desdeDeb ? `${desdeDeb}T00:00:00.000Z` : undefined,
      hasta: hastaDeb ? `${hastaDeb}T23:59:59.999Z` : undefined,
    }),
    [pageSize, filtroTipoDeb, desdeDeb, hastaDeb],
  );

  useEffect(() => {
    (async () => {
      try {
        const res = await tiposInteraccionService.getAll();
        setTipos(res.items.filter((t) => t.activo));
        setTipoSeleccionado((actual) => actual || res.items.find((t) => t.activo)?.id || '');
      } catch (e) {
        setError(e.message);
      }
    })();
  }, []);

  const cargar = useCallback(async () => {
    setCargando(true);
    setError(null);
    try {
      const res = await interaccionesService.getByComercio(comercioId, paramsDe(page));
      setLista(res.items);
      setDatos(res);
    } catch (e) {
      setError(e.message);
    } finally {
      setCargando(false);
    }
  }, [comercioId, page, paramsDe]);

  useEffect(() => {
    cargar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [cargar]);

  const cambiarFiltro = (setter) => (e) => {
    setter(e.target.value);
    setPage(1);
  };

  const cambiarPageSize = (n) => {
    setPageSize(n);
    setPage(1);
  };

  const agregar = async (fechaInteraccion, notas) => {
    setError(null);
    try {
      await interaccionesService.create({
        comercioId,
        tipoInteraccionId: tipoSeleccionado,
        fechaInteraccion,
        notas,
      });
      setPage(1);
      const res = await interaccionesService.getByComercio(comercioId, paramsDe(1));
      setLista(res.items);
      setDatos(res);
    } catch (e) {
      setError(e.message);
      throw e;
    }
  };

  return {
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
    setPageSize,
    cambiarPageSize,
    agregar,
  };
}
