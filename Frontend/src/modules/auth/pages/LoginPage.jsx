import { useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import Alert from '../../../shared/components/atoms/Alert';
import Button from '../../../shared/components/atoms/Button';
import Input from '../../../shared/components/atoms/Input';
import PasswordInput from '../../../shared/components/atoms/PasswordInput';
import { useToast } from '../../../shared/context/ToastProvider';
import { useAuth } from '../context/AuthProvider';

export default function LoginPage() {
  const { user, isAuthenticated, login } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();
  const location = useLocation();

  const [usuario, setUsuario] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [enviando, setEnviando] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/comercios" replace />;
  }

  async function onSubmit(e) {
    e.preventDefault();
    setError(null);
    setEnviando(true);
    try {
      const usuarioLogueado = await login({ usuario, password });
      if (usuarioLogueado.debeCambiarPassword) {
        toast.info('Primero definí tu contraseña nueva.');
        navigate('/cambiar-password', { replace: true });
        return;
      }
      toast.success(`¡Bienvenido, ${usuarioLogueado.nombre}!`);
      const destino = location.state?.from?.pathname || '/comercios';
      navigate(destino, { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-100 px-4">
      <div className="w-full max-w-sm rounded-lg border border-border bg-white p-8 shadow-sm">
        <div className="mb-6 text-center">
          <span className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-md bg-primary text-lg font-bold text-white">
            C
          </span>
          <h1 className="text-xl font-semibold text-text">CMS Zoco</h1>
          <p className="mt-1 text-sm text-muted">Ingresá con tu usuario y contraseña</p>
        </div>

        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          {error && <Alert tono="error">{error}</Alert>}

          <div>
            <label htmlFor="usuario" className="mb-1 block text-sm font-medium text-text">
              Usuario
            </label>
            <Input
              id="usuario"
              type="text"
              autoComplete="username"
              value={usuario}
              onChange={(e) => setUsuario(e.target.value)}
              placeholder="admin o ventas"
              required
            />
          </div>

          <div>
            <label htmlFor="password" className="mb-1 block text-sm font-medium text-text">
              Contraseña
            </label>
            <PasswordInput
              id="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
          </div>

          <Button type="submit" variant="primary" className="w-full" disabled={enviando}>
            {enviando ? 'Ingresando…' : 'Ingresar'}
          </Button>
        </form>

        {!user && (
          <p className="mt-4 text-center text-xs text-muted">
            Usuarios iniciales: admin / ventas
          </p>
        )}
      </div>
    </div>
  );
}
