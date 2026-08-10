import { useEffect, useState } from 'react';
import { Ban, Plus, UserRoundCog } from 'lucide-react';
import Alert from '../../../shared/components/atoms/Alert';
import Badge from '../../../shared/components/atoms/Badge';
import Button from '../../../shared/components/atoms/Button';
import EmptyState from '../../../shared/components/atoms/EmptyState';
import IconButton from '../../../shared/components/atoms/IconButton';
import Spinner from '../../../shared/components/atoms/Spinner';
import { useToast } from '../../../shared/context/ToastProvider';
import { useAuth } from '../../auth/context/AuthProvider';
import { authService } from '../../auth/services/authService';
import CuentaNuevaModal from '../components/CuentaNuevaModal';

function BadgeEstado({ activo }) {
  return activo ? (
    <Badge variant="green" tone="soft">Activo</Badge>
  ) : (
    <Badge variant="gray" tone="soft">Inactivo</Badge>
  );
}

export default function CuentasPage() {
  const { user } = useAuth();
  const toast = useToast();
  const [cuentas, setCuentas] = useState([]);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState(null);
  const [modalNueva, setModalNueva] = useState(false);

  const cargar = async () => {
    setCargando(true);
    setError(null);
    try {
      setCuentas(await authService.listarUsuarios());
    } catch (e) {
      setError(e.message);
    } finally {
      setCargando(false);
    }
  };

  useEffect(() => {
    cargar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const desactivar = async (cuenta) => {
    if (!window.confirm(`¿Desactivar la cuenta "${cuenta.nombreUsuario}"?`)) return;
    setError(null);
    try {
      await authService.desactivarUsuario(cuenta.id);
      await cargar();
      toast.success(`Cuenta "${cuenta.nombreUsuario}" desactivada.`);
    } catch (e) {
      setError(e.message);
    }
  };

  // No se puede desactivar la propia cuenta ni otra cuenta de administrador.
  const puedeDesactivar = (cuenta) =>
    cuenta.id !== user?.id && cuenta.rol !== 'Administrador' && cuenta.activo;

  return (
    <div className="mx-auto max-w-5xl">
      <header className="mb-6 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Cuentas</h1>
        <Button variant="primary" onClick={() => setModalNueva(true)}>
          <Plus className="h-4 w-4" /> Nueva cuenta
        </Button>
      </header>

      <section className="rounded-md border border-gray-200 bg-white p-4 sm:p-5">
        {cargando && (
          <div className="flex items-center gap-2 py-6 text-sm text-gray-500">
            <Spinner /> Cargando…
          </div>
        )}
        {error && <Alert tono="error">{error}</Alert>}

        {!cargando && cuentas.length === 0 && !error && (
          <EmptyState title="No hay cuentas cargadas." description="Creá la primera cuenta para empezar.">
            <Button variant="primary" onClick={() => setModalNueva(true)}>
              <Plus className="h-4 w-4" /> Nueva cuenta
            </Button>
          </EmptyState>
        )}

        {!cargando && cuentas.length > 0 && (
          <div className="overflow-x-auto rounded-md border border-gray-200">
            <table className="w-full min-w-[760px] border-collapse text-left">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50 text-xs font-semibold uppercase tracking-wide text-gray-500">
                  <th className="px-3 py-2.5">Usuario</th>
                  <th className="px-3 py-2.5">Persona</th>
                  <th className="px-3 py-2.5">Rol</th>
                  <th className="px-3 py-2.5">Email</th>
                  <th className="px-3 py-2.5">Teléfono</th>
                  <th className="px-3 py-2.5">Estado</th>
                  <th className="px-3 py-2.5 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody>
                {cuentas.map((cuenta) => {
                  const desactivable = puedeDesactivar(cuenta);
                  const motivo =
                    cuenta.id === user?.id
                      ? 'No podés desactivar tu propia cuenta'
                      : cuenta.rol === 'Administrador'
                        ? 'No se pueden desactivar cuentas de administradores'
                        : !cuenta.activo
                          ? 'La cuenta ya está desactivada'
                          : undefined;

                  return (
                    <tr
                      key={cuenta.id}
                      className={`border-b border-gray-200 transition-colors hover:bg-gray-50 ${!cuenta.activo ? 'bg-gray-50' : ''}`}
                    >
                      <td className="px-3 py-2.5">
                        <code className="rounded bg-gray-100 px-2 py-0.5 text-xs text-gray-700">
                          {cuenta.nombreUsuario}
                        </code>
                        {cuenta.id === user?.id && (
                          <span className="ml-1.5 text-xs text-muted">(vos)</span>
                        )}
                      </td>
                      <td className="px-3 py-2.5 text-sm font-medium text-gray-900">{cuenta.nombre}</td>
                      <td className="px-3 py-2.5">
                        {cuenta.rol === 'Administrador' ? (
                          <Badge variant="indigo" tone="soft">Administrador</Badge>
                        ) : (
                          <Badge variant="blue" tone="soft">Ventas</Badge>
                        )}
                      </td>
                      <td className="px-3 py-2.5 text-sm text-gray-600">{cuenta.email || '—'}</td>
                      <td className="px-3 py-2.5 text-sm text-gray-600">{cuenta.telefono || '—'}</td>
                      <td className="px-3 py-2.5">
                        <BadgeEstado activo={cuenta.activo} />
                      </td>
                      <td className="px-3 py-2.5">
                        <div className="flex items-center justify-end gap-0.5">
                          <IconButton
                            titulo={motivo || 'Desactivar cuenta'}
                            onClick={() => desactivar(cuenta)}
                            disabled={!desactivable}
                            className="hover:text-danger"
                          >
                            <Ban className="h-4 w-4" />
                          </IconButton>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {modalNueva && (
        <CuentaNuevaModal
          onGuardado={() => {
            setModalNueva(false);
            cargar();
          }}
          onClose={() => setModalNueva(false)}
        />
      )}
    </div>
  );
}
