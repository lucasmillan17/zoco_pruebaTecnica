import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Alert from '../../../shared/components/atoms/Alert';
import Button from '../../../shared/components/atoms/Button';
import Input from '../../../shared/components/atoms/Input';
import PasswordInput from '../../../shared/components/atoms/PasswordInput';
import { useAuth } from '../context/AuthProvider';
import { authService } from '../services/authService';

export default function CambiarPasswordPage() {
  const { user, actualizarUsuario, logout } = useAuth();
  const navigate = useNavigate();

  const [passwordActual, setPasswordActual] = useState('');
  const [passwordNueva, setPasswordNueva] = useState('');
  const [confirmacion, setConfirmacion] = useState('');
  const [error, setError] = useState(null);
  const [enviando, setEnviando] = useState(false);

  const forzado = Boolean(user?.debeCambiarPassword);

  async function onSubmit(e) {
    e.preventDefault();
    setError(null);

    if (passwordNueva !== confirmacion) {
      setError('La confirmación no coincide con la nueva contraseña.');
      return;
    }

    setEnviando(true);
    try {
      const usuario = await authService.cambiarPassword({ passwordActual, passwordNueva });
      actualizarUsuario(usuario);
      navigate('/comercios', { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-100 px-4">
      <div className="w-full max-w-md rounded-lg border border-border bg-white p-8 shadow-sm">
        <div className="mb-6 text-center">
          <span className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-md bg-primary text-lg font-bold text-white">
            C
          </span>
          <h1 className="text-xl font-semibold text-text">
            {forzado ? 'Cambiá tu contraseña' : 'Cambiar contraseña'}
          </h1>
          <p className="mt-1 text-sm text-muted">
            {forzado
              ? 'Antes de continuar, necesitás definir una contraseña nueva.'
              : `Cuenta: ${user?.nombreUsuario}`}
          </p>
        </div>

        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          {error && <Alert tono="error">{error}</Alert>}

          <div>
            <label htmlFor="passwordActual" className="mb-1 block text-sm font-medium text-text">
              Contraseña actual
            </label>
            <PasswordInput
              id="passwordActual"
              autoComplete="current-password"
              value={passwordActual}
              onChange={(e) => setPasswordActual(e.target.value)}
              placeholder="••••••••"
              required
            />
          </div>

          <div>
            <label htmlFor="passwordNueva" className="mb-1 block text-sm font-medium text-text">
              Contraseña nueva
            </label>
            <PasswordInput
              id="passwordNueva"
              autoComplete="new-password"
              value={passwordNueva}
              onChange={(e) => setPasswordNueva(e.target.value)}
              placeholder="Mínimo 6 caracteres"
              required
            />
          </div>

          <div>
            <label htmlFor="confirmacion" className="mb-1 block text-sm font-medium text-text">
              Confirmar contraseña nueva
            </label>
            <PasswordInput
              id="confirmacion"
              autoComplete="new-password"
              value={confirmacion}
              onChange={(e) => setConfirmacion(e.target.value)}
              placeholder="Repetí la contraseña nueva"
              required
            />
          </div>

          <div className="flex gap-2 pt-1">
            {!forzado && (
              <Button type="button" variant="secondary" className="flex-1" onClick={() => navigate(-1)}>
                Cancelar
              </Button>
            )}
            <Button type="submit" variant="primary" className="flex-1" disabled={enviando}>
              {enviando ? 'Guardando…' : 'Guardar contraseña'}
            </Button>
          </div>
        </form>

        {forzado && (
          <button
            type="button"
            onClick={logout}
            className="mt-4 w-full text-center text-xs text-muted hover:text-text"
          >
            Cerrar sesión y volver al login
          </button>
        )}
      </div>
    </div>
  );
}
